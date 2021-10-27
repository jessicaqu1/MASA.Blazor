using BlazorComponent;
using Microsoft.AspNetCore.Components;
using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MASA.Blazor
{
    public partial class MCalendar : BCalendar, ICalendar
    {
        #region Week

        [Parameter]
        public StringNumber LocaleFirstDayOfYear { get; set; } = 0;

        [Parameter]
        public int MinWeeks { get; set; } = 1;

        [Parameter]
        public bool ShortMonths { get; set; } = true;

        [Parameter]
        public bool ShowMonthOnFirst { get; set; } = true;

        [Parameter]
        public bool ShowWeek { get; set; }

        [Parameter]
        public Func<CalendarTimestamp, bool, string> MonthFormat { get; set; }

        #endregion

        #region Interals

        [Parameter]
        public int MaxDays { get; set; } = 7;

        [Parameter]
        public bool ShortIntervals { get; set; } = true;

        [Parameter]
        public StringNumber IntervalHeight { get; set; } = 48;

        [Parameter]
        public StringNumber IntervalWidth { get; set; } = 60;

        [Parameter]
        public StringNumber IntervalMinutes { get; set; } = 60;

        [Parameter]
        public StringNumber FirstInterval { get; set; } = 0;

        [Parameter]
        public StringNumberDate FirstTime { get; set; }

        [Parameter]
        public StringNumber IntervalCount { get; set; } = 24;

        [Parameter]
        public Func<CalendarTimestamp, bool, string> IntervalFormat { get; set; }

        [Parameter]
        public Func<CalendarTimestamp, object> IntervalStyle { get; set; }

        [Parameter]
        public Func<CalendarTimestamp, bool> ShowIntervalLabel { get; set; }

        #endregion

        protected CalendarTimestamp _lastStart;

        protected CalendarTimestamp _lastEnd;

        protected override void SetComponentClass()
        {
            base.SetComponentClass();

            CssProvider
                .Apply(cssBuilder =>
                {
                    cssBuilder
                       .Add("m-calendar")
                       .AddIf("m-calendar-events", () => !NoEvents);
                })
                .Apply("daybody", cssBuilder =>
                {
                    cssBuilder.Add("m-event-timed-container");
                });

            Action<Dictionary<string, object>> propsAction = props => {
                foreach (var prop in Props)
                {
                    props[prop.Key] = prop.Value;
                }
                props["Start"] = (StringNumberDate)RenderProps.Start.Date;
                props["End"] = (StringNumberDate)RenderProps.End.Date;
                props["Maxdays"] = RenderProps.MaxDays;
                OneOf<string, List<int>> weekDaysProp = RenderProps.WeekDays;
                props["Weekdays"] = weekDaysProp;
                props["Categories"] = RenderProps.Categories;
            };
            AbstractProvider
                .Apply(typeof(ICalendarMonthly), typeof(MCalendarMonthly), propsAction)
                .Apply(typeof(ICalendarCategory), typeof(MCalendarCategory), propsAction)
                .Apply<BCalendarDaily, MCalendarDaily>(propsAction)
                .Apply<BCalendarWeekly, MCalendarWeekly>(propsAction);
        }

        public CalendarTimestamp ParsedValue => Value != null ? 
            CalendarTimestampUtils.ParseTimestamp(Value) : 
            (ParsedStart() ?? Today);

        public int ParsedCategoryDays => 
            CategoryDays.ToInt32() > 0 ? CategoryDays.ToInt32() : 1;

        public override List<int> EventWeekdays => RenderProps.WeekDays;

        public override bool CategoryMode => Type.Equals("category");

        public string Title()
        {
            var start = RenderProps.Start;
            var end = RenderProps.End;
            var spanYears = start.Year != end.Year;
            var spanMonths = spanYears || start.Month != end.Month;

            return spanYears ?
                $"{MonthShortFormatter(start, true)} {start.Year} - {MonthShortFormatter(end, true)} {end.Year}" :
                (spanMonths ? $"{MonthShortFormatter(start, true)} - {MonthShortFormatter(end, true)} {end.Year}" :
                $"{MonthLongFormatter(start, false)} {start.Year}");
        }

        public Func<CalendarTimestamp, bool, string> MonthLongFormatter =>
            GetFormatter(new() { TimeZone = "UTC", Month = "long" });

        public Func<CalendarTimestamp, bool, string> MonthShortFormatter =>
            GetFormatter(new() { TimeZone = "UTC", Month = "short" });

        public List<OneOf<string, Dictionary<string, object>>> ParsedCategories =>
            CalendarParser.GetParsedCategories(Categories, CategoryText);

        public override CalendarRenderProps RenderProps
        {
            get
            {
                var around = ParsedValue;
                Type component;
                var maxDays = MaxDays;
                var weekdays = ParsedWeekdays();
                Categories = Categories.IsT0 && string.IsNullOrWhiteSpace(Categories.AsT0) ||
                    Categories.IsT1 && Categories.AsT1 == null || !Categories.AsT1.Any() ?
                    GetCategories() : Categories;
                var categories = ParsedCategories;
                var start = around;
                var end = around;
                switch (Type)
                {
                    case "month":
                        component = typeof(ICalendarMonthly);
                        start = CalendarTimestampUtils.GetStartOfMonth(around);
                        end = CalendarTimestampUtils.GetendOfMonth(around);
                        break;
                    case "week":
                        component = typeof(BCalendarDaily);
                        start = GetStartOfWeek(around);
                        end = GetEndOfWeek(around);
                        maxDays = 7;
                        break;
                    case "day":
                        component = typeof(BCalendarDaily);
                        maxDays = 1;
                        weekdays = new List<int> { start.WeekDay };
                        break;
                    case "4day":
                        component = typeof(BCalendarDaily);
                        end = CalendarTimestampUtils.RelativeDays(CalendarTimestampUtils.DeepCopy(around), 3);
                        CalendarTimestampUtils.UpdateFormatted(end);
                        maxDays = 4;
                        weekdays = new List<int>
                        {
                            start.WeekDay,
                            (start.WeekDay + 1) % 7,
                            (start.WeekDay + 2) % 7,
                            (start.WeekDay + 3) % 7
                        };
                        break;
                    case "custom-weekly":
                        component = typeof(BCalendarWeekly);
                        start = ParsedStart() ?? around;
                        end = ParsedEnd();
                        break;
                    case "custom-daily":
                        component = typeof(BCalendarDaily);
                        start = ParsedStart() ?? around;
                        end = ParsedEnd();
                        break;
                    case "category":
                        var days = ParsedCategoryDays;

                        component = typeof(ICalendarCategory);
                        end = CalendarTimestampUtils.RelativeDays(CalendarTimestampUtils.DeepCopy(around), days);
                        CalendarTimestampUtils.UpdateFormatted(end);
                        maxDays = days;
                        weekdays = new List<int>();
                        for (int i = 0; i < days; i++)
                        {
                            weekdays.Add((start.WeekDay + i) % 7);
                        }
                        categories = GetCategoryList(categories);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{Type} is not a valid Calendar type");
                }

                return new CalendarRenderProps()
                {
                    Start = start,
                    End = end,
                    MaxDays = maxDays,
                    WeekDays = weekdays,
                    Component = component,
                    Categories = categories
                };
            }
        }

        public List<OneOf<string, Dictionary<string, object>>> GetCategoryList(
            List<OneOf<string, Dictionary<string, object>>> categories)
        {
            if (!NoEvents)
            {
                var categoryMap = new Dictionary<string, CalendarCategoryMap>();
                for (int i = 0; i < categories.Count; i++)
                {
                    var category = categories[i];
                    if (category.IsT1 && category.AsT1.ContainsKey(CalendarParser.CalendarCategoryCategoryName))
                    {
                        category.AsT1.TryGetValue(CalendarParser.CalendarCategoryCategoryName, out var categoryName);
                        categoryMap = AddCategoryMap(i, categoryMap, categoryName.ToString());
                    }
                    else if (category.IsT0)
                        categoryMap = AddCategoryMap(i, categoryMap, category.AsT0);
                }

                if (!CategoryHideDynamic || !CategoryShowAll)
                { 
                    var categoryLength = categories.Count;

                    ParsedEvents.ForEach(ev =>
                    {
                        var category = ev.Category.IsT1 ? CategoryForInvalid : ev.Category.AsT0;

                        if (string.IsNullOrWhiteSpace(category))
                            return;

                        if (categoryMap.ContainsKey(category))
                            categoryMap[category].Count++;
                        else if (!CategoryHideDynamic)
                            categoryMap.Add(category, new() { Index = categoryLength++, Count = 1 });
                    });
                }

                if (!CategoryShowAll)
                    foreach (var category in categoryMap)
                    {
                        if (category.Value.Count == 0)
                            categoryMap.Remove(category.Key);
                    }

                categories.ForEach(x =>
                {
                    if (x.IsT1 && x.AsT1.ContainsKey(CalendarParser.CalendarCategoryCategoryName))
                    {
                        categoryMap.TryGetValue(x.AsT1[CalendarParser.CalendarCategoryCategoryName].ToString(), out var obj);
                        if (obj == null)
                            categories.Remove(x);
                    }
                    else if (x.IsT0)
                    {
                        categoryMap.TryGetValue(x.AsT0, out var obj);
                        if (obj == null)
                            categories.Remove(x);
                    }
                    else
                        categories.Remove(x);
                });
            }

            return categories;
        }

        private static Dictionary<string, CalendarCategoryMap> AddCategoryMap(int index,
            Dictionary<string, CalendarCategoryMap> categoryMap, string categoryNameStr)
        {
            if(string.IsNullOrWhiteSpace(categoryNameStr))
                return categoryMap;

            if (categoryMap.ContainsKey(categoryNameStr))
                categoryMap[categoryNameStr] = new() { Index = index, Count = 0 };
            else
                categoryMap.Add(categoryNameStr, new() { Index = index, Count = 0 });

            return categoryMap;
        }

        private OneOf<string, List<OneOf<string, Dictionary<string, object>>>> GetCategories()
        {
            var res = new List<OneOf<string, Dictionary<string, object>>>();
            Events?.ForEach(x => { res.Add(x); });

            return res;
        }
    }
}
