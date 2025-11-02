# Git LFS 配置指南

当你在 GitHub Desktop 中遇到 "Files too large" 警告时，按照以下步骤配置 Git LFS。

## 📋 快速解决步骤

### 情况 1：文件还未提交（推荐方法）

如果文件还在工作区，尚未添加到暂存区：

1. **安装 Git LFS**（如果还没安装）
   ```powershell
   # 检查是否已安装
   git lfs version
   
   # 如果未安装，使用 winget 安装
   winget install Git.GitLFS
   ```

2. **初始化 Git LFS**
   ```powershell
   git lfs install
   ```

3. **添加文件类型到 .gitattributes**
   - 打开 `.gitattributes` 文件
   - 添加新行（例如对于 `.zip` 文件）：
   ```
   *.zip filter=lfs diff=lfs merge=lfs -text
   ```

4. **添加文件到暂存区**
   ```powershell
   git add .gitattributes
   git add "文件路径"
   ```

5. **验证文件已被 LFS 跟踪**
   ```powershell
   git lfs ls-files
   ```

### 情况 2：文件已添加到暂存区但未提交

如果文件已经在暂存区（staged），需要先取消暂存：

1. **取消暂存**
   ```powershell
   git reset HEAD "文件路径"
   ```

2. **确保 .gitattributes 包含该文件类型**
   - 编辑 `.gitattributes`，添加对应的文件类型规则

3. **重新添加文件**
   ```powershell
   git add .gitattributes
   git add "文件路径"
   ```

4. **验证**
   ```powershell
   git lfs ls-files
   ```

### 情况 3：文件已经提交到本地仓库

如果文件已经提交到本地仓库，需要迁移历史记录：

**⚠️ 警告：这会重写 Git 历史，如果有团队协作，请谨慎操作！**

1. **迁移特定文件类型**
   ```powershell
   git lfs migrate import --include="*.zip" --everything
   ```
   或迁移特定文件：
   ```powershell
   git lfs migrate import --include="路径/文件名.zip" --everything
   ```

2. **推送到远程仓库**
   ```powershell
   git push --force-with-lease
   ```

## 📝 常见文件类型的 .gitattributes 配置

在 `.gitattributes` 文件中添加以下规则：

```gitattributes
# 压缩文件
*.zip filter=lfs diff=lfs merge=lfs -text
*.rar filter=lfs diff=lfs merge=lfs -text
*.7z filter=lfs diff=lfs merge=lfs -text

# 视频文件
*.mp4 filter=lfs diff=lfs merge=lfs -text
*.mov filter=lfs diff=lfs merge=lfs -text
*.avi filter=lfs diff=lfs merge=lfs -text

# 音频文件
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text

# 3D 模型
*.fbx filter=lfs diff=lfs merge=lfs -text
*.obj filter=lfs diff=lfs merge=lfs -text

# 纹理/图片
*.psd filter=lfs diff=lfs merge=lfs -text
*.tga filter=lfs diff=lfs merge=lfs -text

# 字体
*.ttf filter=lfs diff=lfs merge=lfs -text
*.otf filter=lfs diff=lfs merge=lfs -text
```

## 🔍 常用命令

```powershell
# 检查 Git LFS 版本
git lfs version

# 初始化 Git LFS
git lfs install

# 查看当前被 LFS 跟踪的文件
git lfs ls-files

# 检查特定文件是否被 LFS 跟踪
git lfs ls-files "文件路径"

# 查看 LFS 配置
git lfs env

# 迁移已跟踪的文件到 LFS（重写历史）
git lfs migrate import --include="*.扩展名" --everything
```

## ⚙️ 在 GitHub Desktop 中的操作流程

1. **看到警告对话框时**：
   - 点击 "Cancel" 按钮（不要点击 "Commit anyway"）
   
2. **在终端中配置 LFS**：
   - 打开 PowerShell
   - 进入项目目录
   - 按照上面的步骤配置

3. **返回 GitHub Desktop**：
   - 刷新或重新打开仓库
   - 警告应该消失
   - 正常提交即可

## 🎯 完整示例：添加新的文件类型

假设遇到新的 `.gz` 压缩文件超过 100MB：

```powershell
# 1. 确保 Git LFS 已安装并初始化
git lfs install

# 2. 在 .gitattributes 中添加
# *.gz filter=lfs diff=lfs merge=lfs -text

# 3. 添加配置文件和目标文件
git add .gitattributes
git add "文件路径/example.gz"

# 4. 验证
git lfs ls-files

# 5. 提交
git commit -m "添加 .gz 文件到 LFS"
```

## 📌 重要提示

1. **Git LFS 免费配额**：
   - GitHub 免费账户：1 GB 存储空间 + 1 GB 带宽/月
   - 超出需要购买数据包或考虑其他方案

2. **团队协作**：
   - 所有团队成员都需要安装 Git LFS
   - 确保 `.gitattributes` 文件被提交到仓库

3. **性能考虑**：
   - 首次推送大文件会较慢
   - 拉取时 LFS 文件会自动下载

4. **不要提交的文件**：
   - 某些大文件可能根本不应该提交（如构建产物）
   - 考虑添加到 `.gitignore` 而不是 LFS

## 🔗 参考资源

- [Git LFS 官方文档](https://git-lfs.github.com/)
- [GitHub LFS 文档](https://docs.github.com/en/repositories/working-with-files/managing-large-files)
- [Unity Git 最佳实践](https://docs.unity3d.com/Manual/UnityCloudBuildGit.html)

