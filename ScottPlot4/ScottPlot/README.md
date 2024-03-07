**ScottPlot is a free and open-source plotting library for .NET** that makes it easy to interactively display large datasets.

[![](raw.githubusercontent.com/ScottPlot/ScottPlot/master/dev/graphics/ScottPlot.gif)](https://scottplot.net)

The [**ScottPlot Cookbook**](scottplot.net/cookbook/4.1/) demonstrates how to create line plots, bar charts, pie graphs, scatter plots, and more with just a few lines of code.

## Quickstart

```cs
double[] xs = new double[] {1, 2, 3, 4, 5};
double[] ys = new double[] {1, 4, 9, 16, 25};

var plt = new ScottPlot.Plot(400, 300);
plt.AddScatter(xs, ys);
plt.SaveFig("console.png");
```

![](raw.githubusercontent.com/ScottPlot/ScottPlot/master/dev/graphics/console-quickstart.png)

## Windows Forms Quickstart

Drop a `FormsPlot` from the toolbox onto your form and add the following to your start-up sequence:

```cs
double[] xs = new double[] {1, 2, 3, 4, 5};
double[] ys = new double[] {1, 4, 9, 16, 25};
formsPlot1.Plot.AddScatter(xs, ys);
formsPlot1.Refresh();
```

![](raw.githubusercontent.com/ScottPlot/ScottPlot/master/dev/graphics/winforms-quickstart.png)

## More Quickstarts

* [**Console Application** Quickstart](scottplot.net/quickstart/console/)
* [**Windows Forms** Quickstart](scottplot.net/quickstart/winforms/)
* [**WPF** Quickstart](scottplot.net/quickstart/wpf/)
* [**Avalonia** Quickstart](scottplot.net/quickstart/avalonia/)
* [**Eto** Quickstart](scottplot.net/quickstart/eto/)
* [**Powershell** Quickstart](scottplot.net/quickstart/powershell/)
* [**Interactive Notebook** Quickstart](scottplot.net/quickstart/notebook/)

## Interactive Demo

The [**ScottPlot Demo**](scottplot.net/demo/) allows you to run these examples interactively.

## ScottPlot Cookbook

The [**ScottPlot Cookbook**](scottplot.net/cookbook/4.1/) demonstrates how to create line plots, bar charts, pie graphs, scatter plots, and more with just a few lines of code.

[![](raw.githubusercontent.com/ScottPlot/ScottPlot/master/dev/graphics/cookbook.jpg)](https://scottplot.net/cookbook/4.1/)

## Supported Platforms

### .NET Versions

* .NET Standard 2.0
* .NET Framework 4.6.2 and newer
* .NET (Core) 6 and newer ([compatibility notes](scottplot.net/faq/dependencies/))

### Operating Systems

ScottPlot 4 is supported anywhere `System.Drawing.Common` is.

* Windows
* Linux ([extra setup may be required](scottplot.net/faq/dependencies/))
* MacOS ([extra setup may be required](scottplot.net/faq/dependencies/))

ScottPlot 5 ([in development](github.com/scottplot/scottplot)) uses SkiaSharp for improved cross-platform support for .NET 7 and later.

# Interactive ScottPlot Controls

ScottPlot WinForms control: www.nuget.org/packages/ScottPlot.WinForms

ScottPlot WPF control: www.nuget.org/packages/ScottPlot.WPF

ScottPlot Avalonia control: www.nuget.org/packages/ScottPlot.Avalonia