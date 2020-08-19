using System;
using Datory;
using Datory.Annotations;
using SSCMS.Form.Utils;

namespace SSCMS.Form.Models
{
    [DataTable(FormUtils.TableNameData)]
    public class DataInfo : Entity
    {
        [DataColumn]
        public int FormId { get; set; }

        [DataColumn]
        public bool IsReplied { get; set; }

        [DataColumn]
        public DateTime? ReplyDate { get; set; }

        [DataColumn(Text = true)]
        public string ReplyContent { get; set; }
    }
}
