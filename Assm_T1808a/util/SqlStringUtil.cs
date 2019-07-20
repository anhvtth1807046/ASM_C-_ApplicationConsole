using System;
using System.Collections.Generic;
using System.Text;

namespace Assm_T1808a.util
{
    class SqlStringUtil
    {
        public String InsertString(Type type)
        {
            var fieldStr = new StringBuilder();
            var fieldValue = new StringBuilder();
            foreach (var t in type.GetProperties())
            {
                if (fieldStr.Length > 0 && fieldValue.Length > 0)
                {
                    fieldStr.Append(", ");
                    fieldValue.Append(", ");
                }
                fieldStr.Append(t.Name);
                fieldValue.Append($"@{t.Name}");
            }
            return $"Insert into {type.Name} ({fieldStr}) values ({fieldValue})";
        }

        public String SelectString(Type type)
        {
            return $"Select * form {type.Name}";
        }

        public String UpdateString(Type type)
        {
            var strBuilder = new StringBuilder();
            foreach (var field in type.GetProperties())
            {
                if (strBuilder.Length > 0)
                {
                    strBuilder.Append(", ");
                }
                strBuilder.Append($"{field.Name} = @{field.Name}");
            }
            
            return $"Update {type.Name} set {strBuilder}";
        }
        
        
    }
}
