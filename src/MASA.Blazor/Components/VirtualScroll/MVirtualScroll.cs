using BlazorComponent;
using Microsoft.AspNetCore.Components;
using System;

namespace MASA.Blazor
{
    public class MVirtualScroll<TItem> : BVirtualScroll<TItem>, IMMeasurable
    {
        [Parameter]
        public StringNumber Bench { get; set; } = 0;

        [Parameter]
        public StringNumber ItemHeight { get; set; } = 1;

        [Parameter]
        public StringNumber Height { get; set; }

        [Parameter]
        public StringNumber MaxHeight { get; set; }

        [Parameter]
        public StringNumber MaxWidth { get; set; }

        [Parameter]
        public StringNumber MinHeight { get; set; }

        [Parameter]
        public StringNumber MinWidth { get; set; }

        [Parameter]
        public StringNumber Width { get; set; }

        int _first = 0;

        int _last = 0;

        int _scrollTop = 0;

        int ItemHeightNumber
        {
            get
            {
                int.TryParse(ItemHeight.ToString(), out var height);
                return height;
            }
        }

        int BenchNumber
        {
            get
            {
                int.TryParse(Bench.ToString(), out var bench);
                return bench;
            }
        }

        protected override int FirstToRender => Math.Max(0, _first - BenchNumber);

        protected override int LastToRender => Math.Min(Items.Count, _last + BenchNumber);

        int GetFirst()
        {
            return (int)Math.Floor(_scrollTop * 1.0f / ItemHeightNumber);
        }

        int GetLast(int first)
        {
            decimal.TryParse(Height?.ToString(), out var height);
            return first + (int)(Math.Ceiling(height / ItemHeightNumber));
        }

        protected override void OnScroll(int scroolTop)
        {
            _scrollTop = scroolTop;
            _first = GetFirst();
            _last = GetLast(_first);
        }

        protected override void SetComponentClass()
        {
            CssProvider.Apply(cssbuilder =>
            {
                cssbuilder.Add("m-virtual-scroll");
            }, styleBuilder =>
            {
                styleBuilder.AddMeasurable(this);
            })
            .Apply("container", cssbuilder =>
            {
                cssbuilder.Add("m-virtual-scroll__container");
            }, styleBuilder =>
            {
                styleBuilder.Add(() => $"height:{Items.Count * ItemHeightNumber}px");
            })
            .Apply("item", cssbuilder =>
            {
                cssbuilder.Add("m-virtual-scroll__item");
            });

            _last = GetLast(0);
        }

        protected override void OnRenderItem(int index, TItem item)
        {
            index = index + FirstToRender;

            CssProvider.Remove("item");
            CssProvider.Apply("item", cssbuilder =>
            {
                cssbuilder.Add("m-virtual-scroll__item");
            }, styleBuilder =>
            {
                styleBuilder.Add(() => $"top:{index * ItemHeightNumber}px");
            });
        }
    }
}
