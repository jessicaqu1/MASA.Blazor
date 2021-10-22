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
        private const int DayMax = 12;
        private const int DaysInMonthMin = 28;
        private const int DaysInMonthMax = 31;
        private const int MonthMax = 12;
        private const int MonthMin = 1;
        private const int DayMin = 1;
        private const int DaysInWeek = 7;
        private const int MinutesInHour = 60;
        private const int MinuteMax = 59;
        private const int MinutesInDay = 24 * 60;
        private const int HoutsInDay = 24;
        private const int HourMax = 23;
        private const int FirstHour = 0;
        private const int OffsetYear = 10000;
        private const int OffsetMonth = 100;
        private const int OffsetHour = 100;
        private const int OffsetTime = 10000;
        private static int[] DaysInMonth = new int[13] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        private static int[] DaysInMonthLeap = new int[13] { 0, 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public RenderFragment IntervalHeaderContent { get; set; }

        [Parameter]
        public RenderFragment<CalendarTimestamp> DayLabelHeaderContent { get; set; }

        [Parameter]
        public RenderFragment<DayHeaderContent> DayHeaderContent { get; set; }

        [Parameter]
        public bool Dark { get; set; }

        [Parameter]
        public bool Light { get; set; }

        [CascadingParameter]
        public IThemeable Themeable { get; set; }

        public bool IsDark => Dark ?
            true :
            (Light ? false : Themeable != null && Themeable.IsDark);

        [Parameter]
        public StringNumberDate Value { get; set; }

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
        public Func<object> MonthFormat { get; set; }

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
        public Func<object> IntervalStyle { get; set; }

        [Parameter]
        public Func<CalendarTimestamp, bool> ShowIntervalLabel { get; set; }

        [Parameter]
        public List<OneOf<string, CalendarCategory>> Categories { get; set; }

        [Parameter]
        public OneOf<string, Func<OneOf<string, CalendarCategory>, string>> CategoryText { get; set; }

        [Parameter]
        public bool CategoryHideDynamic { get; set; }

        [Parameter]
        public bool CategoryShowAll { get; set; }

        [Parameter]
        public string CategoryForInvalid { get; set; } = string.Empty;

        [Parameter]
        public StringNumber CategoryDays { get; set; }

        [Parameter]
        public StringNumberDate Start { get; set; } = DateTime.Now;

        [Parameter]
        public StringNumberDate End { get; set; }

        [Parameter]
        public List<int> WeekDays { get; set; } = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

        [Parameter]
        public bool HideHeader { get; set; }

        [Parameter]
        public bool ShortWeekdays { get; set; }

        [Parameter]
        public Func<CalendarTimestamp, bool, string> WeekdayFormat { get; set; }

        [Parameter]
        public Func<CalendarTimestamp, bool, string> DayFormat { get; set; }

        [Parameter]
        public List<Dictionary<string, StringNumberDate>> Events { get; set; }

        [Parameter]
        public StringNumberDate EventStart { get; set; }

        [Parameter]
        public StringNumberDate EventEnd { get; set; }

        [Parameter]
        public string Color { get; set; }

        [Parameter]
        public string Locale { get; set; }

        public OneOf<string, Func<Dictionary<string, StringNumberDate>, bool>> EventTimed = "timed";

        protected StringNumberDate _lastStart = null;

        protected StringNumberDate _lastEnd = null;

        protected CalendarTimestamp Today => ParseTimestamp(Convert.ToDateTime("0000-00-00 00:00"));

        protected CalendarTimestamp Now => ParseTimestamp(Convert.ToDateTime("0000-00-00 00:00"));

        protected double _scrollPush = 0;

        protected List<CalendarTimestamp> _days = new();

        protected List<List<CalendarTimestamp>> _intervals = new();

        protected string _currentLocale => Locale; //TODO $vuetify.lang.current

        protected override void SetComponentClass()
        {
            var render = RenderProps();

            CssProvider
                .Apply(cssBuilder =>
                {
                    cssBuilder
                       .Add("m-calendar")
                       .AddIf("m-calendar-events", () => !NoEvents)
                       .Add(GetCalendarClassByType())
                       .AddTheme(IsDark);
                })
                .Apply("dailyHead", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__head");
                }, styleBuilder =>
                {
                    styleBuilder
                        .Add($"marginRight:{_scrollPush}px");
                })
                .Apply("dailyIntervals", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__intervals-head");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddWidth(IntervalWidth);
                })
                .Apply("dailyHeadDay", cssBuilder =>
                {
                    var timestamp = _days?[cssBuilder.Index];
                    cssBuilder
                        .Add("m-calendar-daily_head-day")
                        .AddIf("m-present", () => timestamp?.Present ?? false)
                        .AddIf("m-past", () => timestamp?.Past ?? false)
                        .AddIf("m-future", () => timestamp?.Future ?? false);
                })
                .Apply("dailyHeadWeekday", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily_head-weekday");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddTextColor((_days?[styleBuilder.Index]?.Present ?? false) ? Color : string.Empty);
                })
                .Apply("dailyHeadDayLabel", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily_head-day-label");
                }).Apply("dailyBody", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__body");
                }).Apply("scrollArea", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__scroll-area");
                }).Apply("dailyPane", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__pane");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddHeight(IntervalCount.ToInt32() * IntervalHeight.ToInt32());
                }).Apply("dailyDayContainer", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__day-container");
                })
                .Apply("dailyIntervalsBody", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__intervals-body");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddWidth(IntervalWidth);
                })
                .Apply("dailyIntervalsLabel", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__interval");
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddHeight(IntervalHeight);
                })
                .Apply("dailyIntervalsText", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-daily__interval-text");
                })
                .Apply("weekly", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-weekly")
                        .AddTheme(IsDark);
                })
                .Apply("weeklyHead", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-weekly__head");
                })
                .Apply("weeklyHeadnumber", cssBuilder =>
                {
                    cssBuilder
                        .Add("m-calendar-weekly__head-weeknumber");
                })
                .Apply("weeklyHeadDay", cssBuilder =>
                {
                    var timestamp = _days?[cssBuilder.Index];
                    var outside = IsOutside(timestamp);
                    cssBuilder
                        .Add("m-calendar-weekly__head-weekday")
                        .AddIf("m-present", () => timestamp?.Present ?? false)
                        .AddIf("m-past", () => timestamp?.Past ?? false)
                        .AddIf("m-future", () => timestamp?.Future ?? false)
                        .AddIf("m-outside", () => outside);
                }, styleBuilder =>
                {
                    styleBuilder
                        .AddTextColor((_days?[styleBuilder.Index]?.Present ?? false) ? Color : string.Empty);
                });

            AbstractProvider
                .Apply<BButton, MButton>(props =>
                {
                    props.TryGetValue("ItemIndex", out var itemIndexStr);
                    var itemIndex = Convert.ToInt32(itemIndexStr);
                    var day = _days?[itemIndex];

                    props[nameof(MButton.Color)] = (day?.Present ?? false) ? Color : "transparent";
                    props[nameof(MButton.Fab)] = true;
                    props[nameof(MButton.Depressed)] = true;
                    //TODO On
                });
        }

        private string GetCalendarClassByType()
        {
            var classStr = string.Empty;
            switch (Type)
            {
                case "month":
                    classStr = "m-calendar-monthly m-calendar-weekly";
                    break;
                case "week":
                case "day":
                case "4day":
                case "custom-daily":
                    classStr = "m-calendar-daily";
                    break;
                case "custom-weekly":
                    classStr = "m-calendar-weekly";
                    break;
                case "category":
                    classStr = "m-calendar-daily m-calendar-category";
                    break;
            }

            return classStr;
        }

        protected bool NoEvents => Events.Count == 0;

        protected CalendarTimestamp ParsedValue()
        {
            var date = Value.ValidateTimestamp() ?
                Value.ToDateTime() : Start.ToDateTime();

            return ParseTimestamp(date);
        }

        protected CalendarTimestamp ParseTimestamp(StringNumberDate dateTime)
        {
            if (!dateTime.ValidateTimestamp())
                return null;

            var date = dateTime.ToDateTime();
            return new CalendarTimestamp
            {
                Date = date.Date.ToString(),
                Day = date.Day,
                WeekDay = (int)date.DayOfWeek,
                Hour = date.Hour,
                Minute = date.Minute,
                Month = date.Month,
                Time = date.ToString("HH:mm"),
                Year = date.Year
            };
        }

        public CalendarRenderProps RenderProps()
        {
            var around = ParsedValue();
            var maxDays = MaxDays;
            var weekdays = WeekDays;
            var categories = ParsedCategories();
            var start = around;
            var end = around;
            switch (Type)
            {
                case "month":
                    start = GetStartOfMonth(around);
                    end = GetendOfMonth(around);
                    break;
                case "week":
                    start = GetStartOfMonth(around);
                    end = GetendOfMonth(around);
                    maxDays = 7;
                    break;
                case "day":
                    maxDays = 1;
                    weekdays = new List<int> { start.WeekDay };
                    break;
                case "4day":
                    end = RelativeDays(around.Clone() as CalendarTimestamp, 3);
                    UpdateFormatted(end);
                    maxDays = 4;
                    weekdays = new List<int>
                    {
                        start.WeekDay,
                        (start.WeekDay + 1) % 7,
                        (start.WeekDay + 2) % 7,
                        (start.WeekDay + 3) % 7
                    };
                    break;
                case "custom-daily":
                case "custom-weekly":
                    start = ParseTimestamp(Start);
                    end = ParseTimestamp(End);
                    break;
                case "category":
                    var days = CategoryDays.ToInt32();
                    maxDays = days < 1 ? 1 : days;
                    end = RelativeDays(around.Clone() as CalendarTimestamp, 3);
                    UpdateFormatted(end);
                    weekdays = new List<int>();
                    for (int i = 0; i < days; i++)
                    {
                        weekdays.Add((start.WeekDay + i) % 7);
                    }
                    categories = GetCategoryList(categories);

                    break;
            }

            return new CalendarRenderProps() { Start = start, End = end, MaxDays = maxDays, WeekDays = weekdays };
        }

        protected CalendarTimestamp GetStartOfMonth(CalendarTimestamp timestamp)
        {
            var start = timestamp.Clone() as CalendarTimestamp;
            start.Day = DayMin;
            UpdateWeekday(start);
            UpdateFormatted(start);

            return start;
        }

        protected CalendarTimestamp GetendOfMonth(CalendarTimestamp timestamp)
        {
            var end = timestamp.Clone() as CalendarTimestamp;
            end.Day = DaysInMonths(end.Year, end.Month);
            UpdateWeekday(end);
            UpdateFormatted(end);

            return end;
        }

        protected int DaysInMonths(int year, int month)
        {
            return ((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0) ?
                DaysInMonthLeap[month] : DaysInMonth[month];
        }

        protected CalendarTimestamp NextDay(CalendarTimestamp timestamp)
        {
            timestamp.Day++;
            timestamp.WeekDay = (timestamp.WeekDay + 1) % DaysInWeek;
            if (timestamp.Day > DaysInMonthMin &&
                timestamp.Day > DaysInMonths(timestamp.Year, timestamp.Month))
            {
                timestamp.Day = DayMin;
                timestamp.Month++;
                if (timestamp.Month > MonthMax)
                {
                    timestamp.Month = MonthMin;
                    timestamp.Year++;
                }
            }

            return timestamp;
        }

        protected CalendarTimestamp RelativeDays(
            CalendarTimestamp timestamp, int days = 1)
        {
            while (--days >= 0)
                timestamp = NextDay(timestamp);

            return timestamp;
        }

        protected CalendarTimestamp UpdateWeekday(CalendarTimestamp timestamp)
        {
            timestamp.WeekDay = GetWeekday(timestamp);

            return timestamp;
        }

        protected int GetWeekday(CalendarTimestamp timestamp)
        {
            if (timestamp.HasDay)
            {
                var k = timestamp.Day;
                var m = ((timestamp.Month + 9) % DayMax) + 1;
                var c = Math.Floor((double)timestamp.Year / 100);
                var y = (timestamp.Year % 100) - (timestamp.Month <= 2 ? 1 : 0);

                return (int)(((k + Math.Floor(2.6 * m - 0.2) - 2 * c + y +
                    Math.Floor((double)(y / 4)) + Math.Floor(c / 4)) % 7) + 7) % 7;
            }

            return timestamp.WeekDay;
        }

        protected CalendarTimestamp UpdateFormatted(CalendarTimestamp timestamp)
        {
            timestamp.Time = GetTime(timestamp);
            timestamp.Date = GetDate(timestamp);

            return timestamp;
        }

        protected string GetTime(CalendarTimestamp timestamp)
        {
            return !timestamp.HasTime ? string.Empty :
                $"{PadNumber(timestamp.Hour, 2)}:{PadNumber(timestamp.Minute, 2)}";
        }

        protected string GetDate(CalendarTimestamp timestamp)
        {
            var str = $"{PadNumber(timestamp.Year, 4)}:{PadNumber(timestamp.Month, 2)}";
            if (timestamp.HasDay)
                str += $"-{PadNumber(timestamp.Day, 2)}";

            return str;
        }

        protected string PadNumber(int x, int length)
        {
            var padded = x.ToString();
            while (padded.Length < length)
            {
                padded = "0" + padded;
            }

            return padded;
        }

        protected List<OneOf<string, CalendarCategory>> GetCategoryList(List<OneOf<string, CalendarCategory>> categories)
        {
            if (!NoEvents)
            {
                var categoryMap = new Dictionary<string, CalendarCategoryMap>();
                for (int i = 0; i < categories.Count; i++)
                {
                    categories[i].Match(
                        t0 => {
                            categoryMap[t0] = new CalendarCategoryMap { Index = i, Count = 0 };

                            return true;
                        },
                        t1 => {
                            if (!string.IsNullOrWhiteSpace(t1.CategoryName))
                                categoryMap[t1.CategoryName] = new CalendarCategoryMap { Index = i, Count = 0 };

                            return true;
                        });
                }

                if (CategoryHideDynamic || !CategoryShowAll)
                {
                    var categoryLength = categories.Count;

                    ParsedEvents().ForEach(ev =>
                    {
                        var category = ev.Category;
                        if (!category.IsString())
                            category = CategoryForInvalid;

                        var categoryStr = category.ToString();
                        if (string.IsNullOrWhiteSpace(categoryStr))
                            return;

                        if (categoryMap.ContainsKey(categoryStr))
                            categoryMap[categoryStr].Count++;
                        else if (CategoryHideDynamic)
                            categoryMap[categoryStr] = new CalendarCategoryMap
                            {
                                Index = categoryLength++,
                                Count = 1
                            };
                    });
                }
            }

            return categories;
        }

        protected List<CalendarEventParsed> ParsedEvents()
        {
            var res = new List<CalendarEventParsed>();
            var index = 0;
            foreach (var input in Events)
            {
                //TODO Timed
                res.Add(ParseEvent(input, index, EventStart, EventEnd, Type == "category"));
                index++;
            }

            return res;
        }

        protected CalendarEventParsed ParseEvent(Dictionary<string, StringNumberDate> input,
            int index, StringNumberDate startProperty, StringNumberDate endProperty,
            StringBoolean category, bool timed = false)
        {
            var startInput = input[startProperty.ToString()];
            var endInput = input[endProperty.ToString()];
            var startParsed = ParseTimestamp(startInput);
            var endParsed = endInput.ValidateTimestamp() ? ParseTimestamp(endInput) : startParsed;
            var start = startInput.IsTimedless() ? UpdateHasTime(startParsed, timed) : startParsed;
            var end = endInput.IsTimedless() ? UpdateHasTime(endParsed, timed) : endParsed;
            var startIdentifier = GetDayIdentifier(start);
            var startTimestampIdentifier = GetTimestampIdentifier(start);
            var endIdentifier = GetDayIdentifier(end);
            var endOffset = start.HasTime ? 0 : 2359;
            var endTimestampIdentifier = GetTimestampIdentifier(end) + endOffset;
            var allDay = !start.HasTime;

            return new CalendarEventParsed
            {
                Input = input,
                Start = start,
                StartIdentifier = startIdentifier,
                StartTimestampIdentifier = startTimestampIdentifier,
                End = end,
                EndIdentifier = endIdentifier,
                EndTimestampIdentifier = endTimestampIdentifier,
                AllDay = allDay,
                Index = index,
                Category = category
            };
        }

        protected CalendarTimestamp UpdateHasTime(CalendarTimestamp timestamp, bool hasTime, CalendarTimestamp now = null)
        {
            if (timestamp.HasTime != hasTime)
            {
                timestamp.HasTime = hasTime;
                if (!hasTime)
                {
                    timestamp.Hour = HourMax;
                    timestamp.Minute = MinuteMax;
                    timestamp.Time = GetTime(timestamp);
                }
                if (now != null)
                {
                    UpdateRelative(timestamp, now, timestamp.HasTime);
                }
            }

            return timestamp;
        }

        protected CalendarTimestamp UpdateRelative(CalendarTimestamp timestamp, CalendarTimestamp now, bool time = false)
        {
            var a = GetDayIdentifier(now);
            var b = GetDayIdentifier(timestamp);
            var present = a == b;

            if (timestamp.HasTime && time && present)
            {
                a = GetTimeIdentifier(now);
                b = GetTimeIdentifier(timestamp);
                present = a == b;
            }

            timestamp.Past = b < a;
            timestamp.Present = present;
            timestamp.Future = b > a;

            return timestamp;
        }

        protected int GetDayIdentifier(CalendarTimestamp timestamp) =>
            timestamp.Year * OffsetYear + timestamp.Month * OffsetMonth + timestamp.Day;

        protected int GetTimeIdentifier(CalendarTimestamp timestamp) =>
            timestamp.Hour * OffsetHour + timestamp.Minute;

        protected int GetTimestampIdentifier(CalendarTimestamp timestamp) =>
            GetDayIdentifier(timestamp) * OffsetTime + GetTimeIdentifier(timestamp);

        //protected bool EventTimedFunction(Dictionary<string, StringNumberDate> @event)
        //{
        //    var ent = EventTimed.Match(
        //        t0 => { var e = @event[t0]; () => false; },
        //        t1 => t1);
        //}

        protected List<OneOf<string, CalendarCategory>> ParsedCategories()
        {
            var res = new List<OneOf<string, CalendarCategory>>();
            foreach (var category in Categories)
            {
                category.Match(
                    t0 =>
                    {
                        res.Add(t0);

                        return true;
                    },
                    t1 =>
                    {
                        var categoryName = !string.IsNullOrWhiteSpace(t1.CategoryName) ?
                            t1.CategoryName : ParsedCategoryText(t1);

                        res.Add(t1);
                        res.Add(categoryName);

                        return true;
                    });
            }

            return res;
        }

        protected string ParsedCategoryText(OneOf<string, CalendarCategory> category)
        {
            return CategoryText.Match(
                t0 => category.Match(
                        t10 => t10,
                        t11 => !string.IsNullOrWhiteSpace(t11.CategoryName) ? t11.CategoryName : t11.Name),
                t1 => t1(category));
        }

        public List<CalendarTimestamp> Days()
        {
            _days = CreateDayList(ParsedStart(), ParsedEnd(), Today, WeekdaySkips());

            return _days;
        }

        protected List<CalendarTimestamp> CreateDayList(CalendarTimestamp start,
            CalendarTimestamp end, CalendarTimestamp now, List<int> weekdaySkips, int max = 42, int min = 0)
        {
            var stop = GetDayIdentifier(end);
            var days = new List<CalendarTimestamp>();
            var current = start.Clone() as CalendarTimestamp;
            var currentIdentifier = 0;
            var stopped = currentIdentifier == stop;

            if (stop < GetDayIdentifier(start))
                throw new ArgumentException("End date is earlier than start date.");

            while ((!stopped || days.Count < min) && days.Count < max)
            {
                currentIdentifier = GetDayIdentifier(start);
                stopped = stopped || currentIdentifier == stop;
                if (weekdaySkips[current.WeekDay] == 0)
                {
                    current = NextDay(current);
                    continue;
                }

                var day = current.Clone() as CalendarTimestamp;
                UpdateFormatted(day);
                UpdateRelative(day, now);
                days.Add(day);
                current = RelativeDays(current, weekdaySkips[current.WeekDay]);
            }

            if (!days.Any())
                throw new ArgumentException("No dates found using specified start date, end date, and weekdays.");

            return days;
        }

        protected CalendarTimestamp ParsedStart() =>
            ParseTimestamp(Start);

        protected CalendarTimestamp ParsedEnd()
        {
            var start = ParsedStart();
            var end = End != null ? ParseTimestamp(End) : start;

            return GetTimestampIdentifier(end) < GetTimestampIdentifier(start) ? start : end;
        }

        protected List<int> WeekdaySkips() =>
            GetWeekdaySkips(WeekDays);

        protected List<int> GetWeekdaySkips(List<int> weekdays)
        {
            var skips = new List<int> { 1, 1, 1, 1, 1, 1, 1 };
            var filled = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < weekdays.Count; i++)
            {
                filled[weekdays[i]] = 1;
            }

            for (int k = 0; k < DaysInWeek; k++)
            {
                var skip = 1;
                for (int j = 0; j < DaysInWeek; j++)
                {
                    var next = (k + j) % DaysInWeek;
                    if (filled[next] > 0)
                        break;

                    skip++;
                }

                skips[k] = filled[k] * skip;
            }

            return skips;
        }

        public Func<CalendarTimestamp, bool, string> WeekdayFormatter()
        {
            if (WeekdayFormat != null)
                return WeekdayFormat;

            var longOptions = new CalendarFormatterOptions { TimeZone = "UTC", Weekday = "long" };
            var shortOptions = new CalendarFormatterOptions { TimeZone = "UTC", Weekday = "short" };

            return CreateNativeLocaleFormatter(_currentLocale, (_tms, @short) => @short ? shortOptions : longOptions);
        }

        public Func<CalendarTimestamp, bool, string> DayFormatter()
        {
            if (DayFormat != null)
                return DayFormat;

            var options = new CalendarFormatterOptions { TimeZone = "UTC", Day = "numeric" };

            return CreateNativeLocaleFormatter(_currentLocale, (_tms, @short) => options);
        }

        protected Func<CalendarTimestamp, bool, string> CreateNativeLocaleFormatter(
            string locale, Func<CalendarTimestamp, bool, object> getOptions)
        {
            Func<CalendarTimestamp, bool, string> emptyFormatter = (_t, _s) => string.Empty;

            //TODO
            return emptyFormatter;
        }

        public List<CalendarTimestamp> TodayWeek()
        {
            var today = Today;
            var start = GetStartOfMonth(Today);
            var end = GetendOfMonth(Today);

            return CreateDayList(start, end, today,
                WeekdaySkips(), WeekDays.Count, WeekDays.Count);
        }

        protected bool IsOutside(CalendarTimestamp day)
        {
            var dayIdentifier = GetDayIdentifier(day);

            return dayIdentifier < GetDayIdentifier(ParsedStart()) ||
                dayIdentifier > GetDayIdentifier(ParsedEnd());
        }

        public List<List<CalendarTimestamp>> Intervals()
        {
            var days = Days();
            var first = FirstMinute();
            var minutes = IntervalMinutes.ToInt32();
            var count = IntervalCount.ToInt32();
            _intervals = new List<List<CalendarTimestamp>>();

            for (var i = 0; i < days.Count; i++)
            {
                _intervals[i].AddRange(CreateIntervalList(days[i], first, minutes, count, Now));
            }

            return _intervals;
        }

        protected int FirstMinute()
        { 
            var time = FirstTime.ToInt32();

            return time >= 0 && time <= MinutesInDay ? time :
                FirstInterval.ToInt32() * IntervalMinutes.ToInt32();
        }

        protected List<CalendarTimestamp> CreateIntervalList(CalendarTimestamp timestamp,
            int first, int minutes, int count, CalendarTimestamp now = null)
        {
            var intervalList = new List<CalendarTimestamp>();
            for (int i = 0; i < count; i++)
            {
                var mins = first + (i * minutes);
                var @int = timestamp.Clone() as CalendarTimestamp;
                intervalList.Add(UpdateMinutes(@int, mins, now));
            }

            return intervalList;
        }

        protected CalendarTimestamp UpdateMinutes(CalendarTimestamp timestamp, 
            int minutes, CalendarTimestamp now = null)
        { 
            timestamp.HasTime = true;
            timestamp.Hour = (int)Math.Floor((double)minutes / MinutesInHour);
            timestamp.Minute = minutes % MinutesInHour;
            timestamp.Time = GetTime(timestamp);

            if (now != null)
                UpdateRelative(timestamp, now, true);

            return timestamp;
        }

        

        

        //protected Func<CalendarTimestamp, bool, string> IntervalFormatter()
        //{
        //    if (IntervalFormat != null)
        //        return IntervalFormat;

        //    var longOptions = new CalendarFormatterOptions { TimeZone = "UTC", Hour = "2-digit", Minute = "2-digit" };
        //}
    }
}
