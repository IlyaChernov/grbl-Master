# grbl-master
Software solution to rule grbl in all aspects.

**Tested only on GRBL 1.1f**

UI is based on grbl-panel https://github.com/gerritv/Grbl-Panel

Improvements are:
* Right implementation of jogging. When you release the jogging button after long press, Grbl will stop movement softly.
* Nice selectors for jogging speed and feed, with normal input fields, so you can use anything you want. Like 11.23mm on 521mm/min.

![image](https://raw.githubusercontent.com/IlyaChernov/grbl-Master/master/images/Jogging.png)

* Numeric inputs for coordinates and jogging distance has built in calculator.

![image](https://raw.githubusercontent.com/IlyaChernov/grbl-Master/master/images/Numeric%20calculator.png)

* Grbl config editor right in ui, without magic numbers and values that you have to send manually.

![image](https://raw.githubusercontent.com/IlyaChernov/grbl-Master/master/images/GrblSettings.png)

* GCode loaded from file can now be edited before running. Editor now has GCode highlighting.

![image](https://raw.githubusercontent.com/IlyaChernov/grbl-Master/master/images/File%20editor.png)

* Easy to edit and use macroses

![image](https://raw.githubusercontent.com/IlyaChernov/grbl-Master/master/images/Macro%20editor.png)

Developed using .NET WPF MVVM as a base and nice libraries and controls, like:
* [Caliburn.Micro](https://caliburnmicro.com/)
* [Fody](https://github.com/Fody/Fody)
* [AvalonEdit](http://avalonedit.net/)
* [ToggleSwitch](https://github.com/ejensen/toggle-switch-control)
* [MaterialDesign](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
* [Reactive extensions](https://github.com/dotnet/reactive)
