﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;
using BlazorComponent.Web;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Element = BlazorComponent.Web.Element;

namespace MASA.Blazor
{
    public partial class MMenu : BMenu
    {
        private string AttachedSelector => Attach ?? ".m-application";

        [Parameter]
        public bool Dark { get; set; }

        protected override void SetComponentClass()
        {
            Transition ??= "m-menu-transition";
            Origin ??= "top left";
            
            CssProvider
                .Apply(css => css.Add("m-menu"))
                .Apply("content", css =>
                {
                    css.Add("m-menu__content")
                        .AddIf("m-menu__content--auto", () => Auto)
                        .AddIf("m-menu__content--fixed", () => ActivatorFixed)
                        .AddIf("menuable__content__active", () => Value)
                        .AddRounded(Tile ? "0" : Rounded)
                        .Add(ContentClass);
                }, style =>
                {
                    style
                        .AddIf($"max-height:{CalculatedMaxHeight}", () => CalculatedMaxHeight != null)
                        .AddIf($"min-width:{CalculatedMinWidth}", () => CalculatedMinWidth != null)
                        .AddIf($"max-width:{CalculatedMaxWidth}", () => CalculatedMaxWidth != null)
                        .Add($"top:{CalculatedTop}")
                        .Add($"left:{CalculatedLeft}")
                        .Add($"transform-origin:{Origin}")
                        .Add($"z-index:{InternalZIndex}")
                        .Add(ContentStyle);
                });
        }

        protected override Task DelContentFrom()
        {
            return JsInvokeAsync(JsInteropConstants.DelElementFrom, ContentRef, AttachedSelector);
        }

        protected override Task MoveContentTo()
        {
            return JsInvokeAsync(JsInteropConstants.AddElementTo, ContentRef, AttachedSelector);
        }
    }
}