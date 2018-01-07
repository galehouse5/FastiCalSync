using System;

namespace FastiCalSync.UI.Extensions
{
    public static class FormattingExtensions
    {
        public static string FormatDuration(this TimeSpan value)
            => value < TimeSpan.FromSeconds(60) ? $"{value.Seconds} sec"
            : value < TimeSpan.FromMinutes(60) ? $"{value.Minutes} min"
            : value < TimeSpan.FromHours(24) ? $"{value.Hours} hr{(value.Hours > 1 ? "s" : null)}"
            : $"{value.Days} day{(value.Days > 1 ? "s" : null)}";
    }
}
