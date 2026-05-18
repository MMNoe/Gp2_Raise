# Gp2 XR Camp Project 3

## 資料夾結構

```
Assets/
├── Scenes/
│   └── SceneForest/    # 森林場景資源（著色器、材質、預製件）
├── Settings/           # URP 渲染管線設定
└── Resources/          # Meta XR 音效設定
```

## Git LFS

大型二進位檔案透過 Git LFS 管理。追蹤的檔案類型：`.png`, `.jpg`, `.jpeg`, `.psd`, `.tga`, `.tiff`, `.exr`, `.hdr`, `.fbx`, `.obj`, `.blend`, `.wav`, `.mp3`, `.ogg`, `.mp4`, `.mov`, `.unitypackage`, `.ttf`, `.otf`, `.bin`。

After cloning, run:
```bash
git lfs install
git lfs pull
```

> **若場景中出現粉紅色物件，代表 Git LFS 尚未安裝或未執行 `git lfs pull`。**

### Windows 安裝步驟

1. 至 [https://git-lfs.com](https://git-lfs.com) 下載並執行安裝程式，或使用 winget：
   ```
   winget install GitHub.GitLFS
   ```
2. 開啟 Git Bash 或命令提示字元，執行：
   ```
   git lfs install
   git lfs pull
   ```

### macOS 安裝步驟

1. 使用 Homebrew 安裝：
   ```
   brew install git-lfs
   ```
2. 執行：
   ```
   git lfs install
   git lfs pull
   ```
