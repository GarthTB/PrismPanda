# [PrismPanda 🐼 棱镜熊猫](https://github.com/GarthTB/PrismPanda)

[![Framework](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
[![Version](https://img.shields.io/badge/release-1.1.1-brightgreen)](https://github.com/GarthTB/PrismPanda/releases)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue)](https://www.apache.org/licenses/LICENSE-2.0)

In various advanced color spaces, effortlessly adjust the chroma of a color bitmap using sliders or numerical inputs, while applying masks to prevent clipping. Regardless of the original bit depth, all adjustments are performed with double-precision floating-point calculations to eliminate quantization errors.

在多种先进的色空间中，用滑块或数值输入来轻松地调整一张彩色位图的彩度，并利用蒙版来防止溢出。无论原始位深如何，都会使用双精度浮点数进行调整，以避免量化误差。

## Supported Colour Spaces 支持的色空间

- HSI
- TSL
- LCH (CIELab)
- LCH (CIELuv)
- JzCzhz
- OKLCH
- HCT

## Release Notes 发布日志

### v1.1.1 - 20250307

- 修复：16位图像的编辑功能

### v1.1.0 - 20250303

- 添加：预览开关

### v1.0.3 - 20250302

- 改用HSL通道的饱和度来蒙版（经测试最佳）。

### v1.0.2 - 20250302

- 改用更柔和的饱和度提升方法，以保护低饱和区域。

### v1.0.1 - 20250302

- 改用HSV通道的饱和度来蒙版。

### v1.0.0 - 20250302

- 发布！