﻿#pragma checksum "..\..\..\..\ToolWindows\MyToolWindowControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "E5700FF72BF65469BC90107C52E863A261AC86EF1E225F19091EBCEE419ED78B"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SetGlobalVersion {
    
    
    /// <summary>
    /// MyToolWindowControl
    /// </summary>
    public partial class MyToolWindowControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal SetGlobalVersion.MyToolWindowControl Set_Version_Number;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid mySolutionDataGrid;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid myProjectsDataGrid;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox VersionMajorEntryName;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox VersionMinorEntryName;
        
        #line default
        #line hidden
        
        
        #line 85 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox BuildNumberEntryName;
        
        #line default
        #line hidden
        
        
        #line 98 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox RevisionNumberEntryName;
        
        #line default
        #line hidden
        
        
        #line 129 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button SetNumbersButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SetGlobalVersion;component/toolwindows/mytoolwindowcontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.Set_Version_Number = ((SetGlobalVersion.MyToolWindowControl)(target));
            return;
            case 2:
            this.mySolutionDataGrid = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 3:
            this.myProjectsDataGrid = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 4:
            this.VersionMajorEntryName = ((System.Windows.Controls.TextBox)(target));
            
            #line 60 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            this.VersionMajorEntryName.LostFocus += new System.Windows.RoutedEventHandler(this.VersionMajorEntryName_LostFocus);
            
            #line default
            #line hidden
            
            #line 61 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            this.VersionMajorEntryName.ManipulationCompleted += new System.EventHandler<System.Windows.Input.ManipulationCompletedEventArgs>(this.VersionMajorEntryName_ManipulationCompleted);
            
            #line default
            #line hidden
            return;
            case 5:
            this.VersionMinorEntryName = ((System.Windows.Controls.TextBox)(target));
            
            #line 74 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            this.VersionMinorEntryName.LostFocus += new System.Windows.RoutedEventHandler(this.VersionMinorEntryName_LostFocus);
            
            #line default
            #line hidden
            
            #line 75 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            this.VersionMinorEntryName.ManipulationCompleted += new System.EventHandler<System.Windows.Input.ManipulationCompletedEventArgs>(this.VersionMinorEntryName_ManipulationCompleted);
            
            #line default
            #line hidden
            return;
            case 6:
            this.BuildNumberEntryName = ((System.Windows.Controls.TextBox)(target));
            
            #line 87 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            this.BuildNumberEntryName.LostFocus += new System.Windows.RoutedEventHandler(this.BuildNumberEntryName_LostFocus);
            
            #line default
            #line hidden
            
            #line 88 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            this.BuildNumberEntryName.ManipulationCompleted += new System.EventHandler<System.Windows.Input.ManipulationCompletedEventArgs>(this.BuildNumberEntryName_ManipulationCompleted);
            
            #line default
            #line hidden
            return;
            case 7:
            this.RevisionNumberEntryName = ((System.Windows.Controls.TextBox)(target));
            
            #line 100 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            this.RevisionNumberEntryName.LostFocus += new System.Windows.RoutedEventHandler(this.RevisionNumberEntryName_LostFocus);
            
            #line default
            #line hidden
            
            #line 101 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            this.RevisionNumberEntryName.ManipulationCompleted += new System.EventHandler<System.Windows.Input.ManipulationCompletedEventArgs>(this.RevisionNumberEntryName_ManipulationCompleted);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 124 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OnRefreshButtonClicked);
            
            #line default
            #line hidden
            return;
            case 9:
            this.SetNumbersButton = ((System.Windows.Controls.Button)(target));
            
            #line 131 "..\..\..\..\ToolWindows\MyToolWindowControl.xaml"
            this.SetNumbersButton.Click += new System.Windows.RoutedEventHandler(this.OnSetNumbersButtonClicked);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

