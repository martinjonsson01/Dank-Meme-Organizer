using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMO.GoogleAPI.Models
{
    public class Status
    {
        /// <summary>
        /// The status code, which should be an enum value of [google.rpc.Code][google.rpc.Code].
        /// </summary>
        public Code Code { get; set; }

        /// <summary>
        /// A list of messages that carry the error details. 
        /// There will be a common set of message types for APIs to use.
        /// </summary>
        public List<string> Details { get; }

        /// <summary>
        /// A developer-facing error message, which should be in English. 
        /// Any user-facing error message should be localized and sent in the [google.rpc.Status.details][google.rpc.Status.details] field, or localized by the client.
        /// </summary>
        public string Message { get; set; }
    }
}
