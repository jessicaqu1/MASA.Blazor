﻿using BlazorComponent;

namespace MASA.Blazor
{
    internal class MBreadcrumbsDivider : BBreadcrumbsDivider
    {
        protected override void SetComponentClass()
        {
            CssProvider.Apply(css => { css.Add("m-breadcrumbs__divider"); });
        }
    }
}