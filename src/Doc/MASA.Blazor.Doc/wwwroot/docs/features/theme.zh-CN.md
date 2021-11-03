---
order: 3
title: 主题
---

轻松更改应用程序的颜色。 重建默认样式表并根据您的特殊需要自定义框架的各个方面。

## 浅色和深色

MASA.Blazor 支持 浅色 light 和 深色 dark 主题。 默认情况下，您的应用程序将使用 **light** 主题。 通过在全局配置类GlobalConfig中配置 **Dark** 属性，你可以很容易的改变主题。
我们需要将全局配置类**BlazorComponent.GlobalConfig**通过构造函数注入或者属性注入到我们的实际业务代码中，并设置Dark属性。

```c#
@page "/themSwitch"
@inject GlobalConfig GlobalConfig

<button @onclick="SetDarkThem">Dark主题</button>
<button @onclick="SetLightThem">Light主题</button>
<button @onclick="SwitchThem">切换主题</button>

@code{
	void SetDarkThem()
	{
		GlobalConfig.Dark = true;
	}

	void SetLightThem()
	{
		GlobalConfig.Dark = false;
	}

	void SwitchThem()
	{
		GlobalConfig.Dark = !GlobalConfig.Dark;
	}
}
```
当你指定一个组件为浅色或暗色时，除非子组件有单独指定主题，所有子组件将继承并应用同样的主题。
