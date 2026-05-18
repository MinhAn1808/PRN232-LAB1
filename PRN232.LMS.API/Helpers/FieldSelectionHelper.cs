using System.Reflection;

namespace PRN232.LMS.API.Helpers
{
    public static class FieldSelectionHelper
    {
        public static List<object> SelectFields<T>(IEnumerable<T> items, string? fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return items.Cast<object>().ToList();
            }

            var requestedFields = fields
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.ToLower())
                .ToHashSet();

            var properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => requestedFields.Contains(p.Name.ToLower()) ||
                            requestedFields.Contains(ToCamelCase(p.Name)))
                .ToList();

            return items.Select(item =>
            {
                var selected = new Dictionary<string, object?>();
                foreach (var property in properties)
                {
                    selected[ToCamelCase(property.Name)] = property.GetValue(item);
                }

                return (object)selected;
            }).ToList();
        }

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return char.ToLowerInvariant(value[0]) + value[1..];
        }
    }
}
