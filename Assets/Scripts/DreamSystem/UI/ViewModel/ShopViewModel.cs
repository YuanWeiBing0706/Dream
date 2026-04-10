using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using DreamManager;
using Struct;
using UnityEngine;
using VContainer;

namespace DreamSystem.UI.ViewModel
{
    public class ShopViewModel : ViewModelBase
    {
        private readonly GameSessionData _sessionData;
        private readonly ResourcesManager _resources;
        private readonly ShopSystem _shopSystem;

        private UniTaskCompletionSource<bool> _closeTcs;

        public List<ShopOffer> Offers { get; private set; } = new();
        public int CurrentGold => _sessionData.CurrentCoinCount;
        public int Chapter { get; private set; }
        public string SelectedOfferId { get; private set; }
        public ResourcesManager Resources => _resources;

        [Inject]
        public ShopViewModel(GameSessionData sessionData, ResourcesManager resources, ShopSystem shopSystem)
        {
            _sessionData = sessionData;
            _resources = resources;
            _shopSystem = shopSystem;
        }

        public void OpenForChapter(int chapter)
        {
            Chapter = chapter;
            _shopSystem.BuildOffersForChapter(chapter);
            Offers = GetVisibleOffers();
            SelectedOfferId = null;
            NotifyRefresh();
        }

        public UniTask WaitForClose()
        {
            _closeTcs = new UniTaskCompletionSource<bool>();
            return _closeTcs.Task;
        }

        public void SelectOffer(string offerId)
        {
            var offer = Offers.FirstOrDefault(x => x.OfferId == offerId);
            if (offer == null || offer.IsSold) return;

            SelectedOfferId = SelectedOfferId == offerId ? null : offerId;
            NotifyRefresh();
        }

        public bool CanConfirmPurchase()
        {
            var offer = GetSelectedOffer();
            if (offer == null || offer.IsSold) return false;
            return _sessionData.CurrentCoinCount >= offer.Price;
        }

        public ShopOffer GetSelectedOffer()
        {
            if (string.IsNullOrWhiteSpace(SelectedOfferId)) return null;
            return Offers.FirstOrDefault(x => x.OfferId == SelectedOfferId);
        }

        public void ConfirmPurchaseSelected()
        {
            var selected = GetSelectedOffer();
            if (selected == null)
            {
                UnityEngine.Debug.LogWarning("[Shop] 尚未选择商品。");
                return;
            }

            if (_shopSystem.TryPurchase(selected.OfferId, out var message))
            {
                UnityEngine.Debug.Log($"[Shop] {message}，剩余金币: {_sessionData.CurrentCoinCount}");
                SelectedOfferId = null;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[Shop] 购买失败: {message}");
            }

            Offers = GetVisibleOffers();
            NotifyRefresh();
        }

        private List<ShopOffer> GetVisibleOffers()
        {
            // 商店内购买后立即从容器移除，GridLayout 会自动前移补位
            return _shopSystem.GetCurrentOffers()
                .Where(x => !x.IsSold)
                .ToList();
        }

        public void ConfirmLeaveShop()
        {
            _closeTcs?.TrySetResult(true);
        }
    }
}
