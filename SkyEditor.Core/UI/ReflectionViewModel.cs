using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SkyEditor.Core.UI
{
    /// <summary>
    /// A view model that can read all public properties
    /// </summary>
    public class ReflectionViewModel : GenericViewModel
    {

        public ReflectionViewModel()
        {
        }

        public ReflectionViewModel(object model, ApplicationViewModel appViewModel) : base(model, appViewModel)
        {
        }

        public override IEnumerable<TypeInfo> GetSupportedTypes()
        {
            return new[] { typeof(object).GetTypeInfo() };
        }

        protected bool IsPropertyTypeSupported(TypeInfo type)
        {
            var supportedTypes = new[] { typeof(string).GetTypeInfo(), typeof(int).GetTypeInfo() };
            return supportedTypes.Contains(type);
            //Todo: add check for custom interface
        }

        public IEnumerable<PropertyInfo> GetProperties()
        {
            var allProperties = Model.GetType().GetTypeInfo().DeclaredProperties;
            return allProperties.Where(x => IsPropertyTypeSupported(x.PropertyType.GetTypeInfo()));
        }

        public Dictionary<string, string> GetPropertyValues()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (var item in GetProperties())
            {
                output.Add(item.Name, item.GetValue(Model).ToString());
            }
            return output;
        }
    }
}
