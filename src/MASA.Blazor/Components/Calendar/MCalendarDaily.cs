using BlazorComponent;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MASA.Blazor
{
    public partial class MCalendarDaily : BCalendarDaily, ICalendarDaily
    {
        [Parameter]
        public bool Dark { get; set; }

        [Parameter]
        public bool Light { get; set; }

        [CascadingParameter]
        public IThemeable Themeable { get; set; }

        public bool IsDark => Dark ?
            true :
            (Light ? false : Themeable != null && Themeable.IsDark);

        protected double _scrollPush = 0;

        protected override void SetComponentClass()
        {
            CssProvider
                .Apply(cssBuilder =>
                {
                    cssBuilder
                       .Add("m-calendar-daily")
                       .AddTheme(IsDark);
                })
                .Apply("head", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__head");
                }, styleBuilder =>
                {
                    styleBuilder
                        .Add($"marginRight:{_scrollPush}px");
                })
                .Apply("intervals", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__intervals-head");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddWidth(IntervalWidth);
                })
                .Apply("headDay", cssBuilder =>
                {
                    var timestamp = _days?[cssBuilder.Index];
                    cssBuilder
                        .Add("m-calendar-daily_head-day")
                        .AddIf("m-present", () => timestamp?.Present ?? false)
                        .AddIf("m-past", () => timestamp?.Past ?? false)
                        .AddIf("m-future", () => timestamp?.Future ?? false);
                })
                .Apply("headWeekday", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily_head-weekday");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddTextColor((_days?[styleBuilder.Index]?.Present ?? false) ? Color : string.Empty);
                })
                .Apply("headDayLabel", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily_head-day-label");
                })
                .Apply("body", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__body");
                })
                .Apply("scrollArea", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__scroll-area");
                })
                .Apply("pane", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__pane");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddHeight(BodyHeight);
                })
                .Apply("dayContainer", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__day-container");
                })
                .Apply("intervalsBody", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__intervals-body");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddWidth(IntervalWidth);
                })
                .Apply("intervalsLabel", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__interval");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddHeight(IntervalHeight);
                })
                .Apply("intervalsText", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__interval-text");
                })
                .Apply("day", cssBuilder =>
                {
                    var timestamp = _days?[cssBuilder.Index];
                    cssBuilder
                        .Add("m-calendar-daily__day")
                        .AddIf("m-present", () => timestamp?.Present ?? false)
                        .AddIf("m-past", () => timestamp?.Past ?? false)
                        .AddIf("m-future", () => timestamp?.Future ?? false);
                })
                .Apply("dayIntervals", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__day-interval");
                }, styleBuilder =>
                {
                    var timestamp = _days?[styleBuilder.Index];
                    var styler = IntervalStyle(timestamp);
                    //TODO ...styler(interval),
                    styleBuilder
                        .AddHeight(IntervalHeight);
                });
            
            AbstractProvider
                .ApplyCalendarDailyDefault()
                .Apply<BButton, MButton>(props =>
                {
                    props.TryGetValue("ItemIndex", out var itemIndexStr);
                    var itemIndex = Convert.ToInt32(itemIndexStr);
                    var day = _days?[itemIndex];

                    props[nameof(MButton.Color)] = (day?.Present ?? false) ? Color : "transparent";
                    props[nameof(MButton.Fab)] = true;
                    props[nameof(MButton.Depressed)] = true;
                    //TODO getMouseEventHandlers
                });
        }



        public string GenIntervalLabel(CalendarTimestamp interval)
        {
            var @short = ShortIntervals;
            var show = ShowIntervalLabel(interval) || ShowIntervalLabelDefault(interval);
            var label = show ? IntervalFormatter(interval, @short) : null;

            return label;
        }

        public List<CalendarTimestamp> GenDayIntervals(int index)
        {
            _intervals ??= Intervals();

            return _intervals.Count() > index ? _intervals[index] : null;
        }
    }
}
