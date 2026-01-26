using Cysharp.Threading.Tasks;
namespace Function.Initialize
{
    public interface IUniTaskStartable
    {
        UniTask AsyncStart();
    }
}