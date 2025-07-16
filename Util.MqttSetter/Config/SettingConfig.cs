using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Util.MqttSetter.Config
{
    internal class SettingConfig
    {
        public string WorkshopId { get; set; } = string.Empty;
        public List<string> DevicePrefixes { get; set; } = [];
        public List<string> TeacherIds { get; set; } = [];
        public List<string> TeamIds { get; set; } = [];
    }
}
