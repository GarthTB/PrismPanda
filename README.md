# [PrismPanda 🐼 棱镜熊猫](https://github.com/GarthTB/PrismPanda)

[![DotNet](https://img.shields.io/badge/.NET-9.0-blue)](https://github.com/GarthTB/PrismPanda)
[![Version](https://img.shields.io/badge/release-0.1.0-brightgreen)](https://github.com/GarthTB/PrismPanda/releases)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue)](https://www.apache.org/licenses/LICENSE-2.0)

In various advanced color spaces, effortlessly adjust the chroma of a color bitmap using sliders or numerical inputs, while applying masks to prevent clipping. Regardless of the original bit depth, all adjustments are performed with double-precision floating-point calculations to eliminate quantization errors.

在多种先进的色空间中，用滑块或数值输入来轻松地调整一张彩色位图的彩度，并利用蒙版来防止溢出。无论原始位深如何，都会使用双精度浮点数进行调整，以避免量化误差。

When the gain is 0, the chroma is 0; when the gain is 2, the chroma is 1. The adjustment range offers unrestricted freedom.

当增益为0时，饱和度就是0；当增益为2时，饱和度就是1。调整范围无拘无束。

## Supported Colour Spaces 支持的色空间

- HSI
- TSL
- LCH (CIELab)
- LCH (CIELuv)
- JzCzhz
- OKLCH
- HCT

## Release Notes 发布日志

### v0.1.0 - 20250128

- 发布！