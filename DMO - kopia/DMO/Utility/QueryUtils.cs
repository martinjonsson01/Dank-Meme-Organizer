using DMO.Services.SettingsServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Search;

namespace DMO.Utility
{
    public static class QueryUtils
    {
        public static SortEntry GetSortEntryFromSettings()
        {
            var sortBy = SettingsService.Instance.SortBy;
            var ascending = SettingsService.Instance.SortDirection == Microsoft.Toolkit.Uwp.UI.SortDirection.Ascending;

            var sortProperty = "System.ItemNameDisplay";
            switch (sortBy)
            {
                case "Name":
                    sortProperty = "System.ItemNameDisplay";
                    break;
                case "Last Modified":
                    sortProperty = "System.DateModified";
                    break;
                case "Created":
                    sortProperty = "System.DateModified"; //TODO: See https://stackoverflow.com/questions/51610494/get-list-of-storagefile-ordered-by-type for why it's not sorting by System.DateCreated
                    break;
            }
            return new SortEntry { PropertyName = sortProperty, AscendingOrder = ascending };
        }
    }
}
