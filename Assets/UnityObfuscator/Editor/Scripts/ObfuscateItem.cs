using System.Collections;
using System.Collections.Generic;
using System;
using Mono.Cecil;


namespace Flower.UnityObfuscator
{
    internal static class ObfuscateItemFactory
    {
        public static NamespaceObfuscateItem Create(string @namespace, ModuleDefinition moduleDefinition)
        {
            return new NamespaceObfuscateItem(@namespace, moduleDefinition);
        }

        public static TypeObfuscateItem Create(TypeDefinition typeDefinition)
        {
            return new TypeObfuscateItem(typeDefinition);
        }

        public static MethodObfuscateItem Create(MethodDefinition methodDefinition)
        {
            return new MethodObfuscateItem(methodDefinition);
        }

        public static PropertyObfuscateItem Create(PropertyDefinition propertyDefinition)
        {
            return new PropertyObfuscateItem(propertyDefinition);
        }

        public static FieldObfuscateItem Create(FieldDefinition fieldDefinition)
        {
            return new FieldObfuscateItem(fieldDefinition);
        }
    }

    internal abstract class BaseObfuscateItem : IEquatable<BaseObfuscateItem>
    {
        protected string moduleName;

        public abstract string Name { get; }

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is BaseObfuscateItem && Equals((BaseObfuscateItem)obj);
        }

        public abstract bool Equals(BaseObfuscateItem other);

        public override string ToString()
        {
            return moduleName;
        }

        public static bool operator ==(BaseObfuscateItem a, BaseObfuscateItem b)
        {
            return a.Equals(b);
        }


        public static bool operator !=(BaseObfuscateItem a, BaseObfuscateItem b)
        {
            return !(a == b);
        }
    }

    internal class NamespaceObfuscateItem : BaseObfuscateItem
    {
        private string @namespace;

        public NamespaceObfuscateItem(string @namespace, ModuleDefinition moduleDefinition)
        {
            this.@namespace = @namespace;
            this.moduleName = moduleDefinition.Name;
        }

        public NamespaceObfuscateItem(string @namespace, string moduleName = "")
        {
            this.@namespace = @namespace;
            this.moduleName = moduleName;
        }

        public override string Name
        {
            get
            {
                return @namespace;
            }
        }

        public override int GetHashCode()
        {
            return @namespace.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is NamespaceObfuscateItem && Equals((NamespaceObfuscateItem)obj);
        }

        public override bool Equals(BaseObfuscateItem other)
        {
            return other is NamespaceObfuscateItem && Equals((NamespaceObfuscateItem)other);
        }

        public bool Equals(NamespaceObfuscateItem other)
        {
            return @namespace == other.@namespace;
        }

        public override string ToString()
        {
            return string.Format("[{0}]{1}", moduleName, @namespace);
        }

        public static bool operator ==(NamespaceObfuscateItem a, NamespaceObfuscateItem b)
        {
            return a.Equals(b);
        }


        public static bool operator !=(NamespaceObfuscateItem a, NamespaceObfuscateItem b)
        {
            return !(a == b);
        }
    }

    internal class TypeObfuscateItem : BaseObfuscateItem
    {
        private string @namespace;
        private string typeName;

        public TypeObfuscateItem(TypeDefinition typeDefinition)
        {
            this.moduleName = typeDefinition.Module.Name;
            this.@namespace = typeDefinition.Namespace;
            this.typeName = typeDefinition.Name;
        }

        public TypeObfuscateItem(string @namespace, string typeName, string moduleName = "")
        {
            this.@namespace = @namespace;
            this.moduleName = moduleName;
            this.typeName = typeName;
        }

        public override string Name
        {
            get
            {
                return typeName;
            }
        }

        public override int GetHashCode()
        {
            return @namespace.GetHashCode() ^ typeName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TypeObfuscateItem && Equals((TypeObfuscateItem)obj);
        }

        public override bool Equals(BaseObfuscateItem other)
        {
            return other is TypeObfuscateItem && Equals((TypeObfuscateItem)other);
        }

        public bool Equals(TypeObfuscateItem other)
        {
            return @namespace == other.@namespace && typeName == other.typeName;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(@namespace))
                return string.Format("[{0}]{1}", moduleName, typeName);
            else
                return string.Format("[{0}]{1}.{2}", moduleName, @namespace, typeName);
        }

        public static bool operator ==(TypeObfuscateItem a, TypeObfuscateItem b)
        {
            return a.Equals(b);
        }


        public static bool operator !=(TypeObfuscateItem a, TypeObfuscateItem b)
        {
            return !(a == b);
        }
    }

    internal class MethodObfuscateItem : BaseObfuscateItem
    {
        private string @namespace;
        private string typeName;
        private string methodName;

        public MethodObfuscateItem(MethodDefinition methodDefinition)
        {
            this.moduleName = methodDefinition.Module.Name;
            this.@namespace = methodDefinition.DeclaringType.Namespace;
            this.typeName = methodDefinition.DeclaringType.Name;
            this.methodName = methodDefinition.Name;
        }

        public MethodObfuscateItem(string @namespace, string typeName, string methodName, string moduleName = "")
        {
            this.@namespace = @namespace;
            this.moduleName = moduleName;
            this.typeName = typeName;
            this.methodName = methodName;
        }

        public override string Name
        {
            get
            {
                return methodName;
            }
        }

        public override int GetHashCode()
        {
            return @namespace.GetHashCode() ^ typeName.GetHashCode() ^ methodName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is MethodObfuscateItem && Equals((MethodObfuscateItem)obj);
        }

        public override bool Equals(BaseObfuscateItem other)
        {
            return other is MethodObfuscateItem && Equals((MethodObfuscateItem)other);
        }

        public bool Equals(MethodObfuscateItem other)
        {
            return @namespace == other.@namespace && typeName == other.typeName && methodName == other.methodName;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(@namespace))
                return string.Format("[{0}]{1}.{2}", moduleName, typeName, methodName);
            else
                return string.Format("[{0}]{1}.{2}.{3}", moduleName, @namespace, typeName, methodName);
        }

        public static bool operator ==(MethodObfuscateItem a, MethodObfuscateItem b)
        {
            return a.Equals(b);
        }


        public static bool operator !=(MethodObfuscateItem a, MethodObfuscateItem b)
        {
            return !(a == b);
        }
    }

    internal class PropertyObfuscateItem : BaseObfuscateItem
    {
        private string @namespace;
        private string typeName;
        private string propertyName;

        public PropertyObfuscateItem(PropertyDefinition propertyDefinition)
        {
            this.moduleName = propertyDefinition.Module.Name;
            this.@namespace = propertyDefinition.DeclaringType.Namespace;
            this.typeName = propertyDefinition.DeclaringType.Name;
            this.propertyName = propertyDefinition.Name;
        }


        public PropertyObfuscateItem(string @namespace, string typeName, string propertyName, string moduleName = "")
        {
            this.@namespace = @namespace;
            this.moduleName = moduleName;
            this.typeName = typeName;
            this.propertyName = propertyName;
        }

        public override int GetHashCode()
        {
            return @namespace.GetHashCode() ^ typeName.GetHashCode() ^ propertyName.GetHashCode();
        }

        public override string Name
        {
            get
            {
                return propertyName;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is PropertyObfuscateItem && Equals((PropertyObfuscateItem)obj);
        }

        public override bool Equals(BaseObfuscateItem other)
        {
            return other is PropertyObfuscateItem && Equals((PropertyObfuscateItem)other);
        }

        public bool Equals(PropertyObfuscateItem other)
        {
            return @namespace == other.@namespace && typeName == other.typeName && propertyName == other.propertyName;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(@namespace))
                return string.Format("[{0}]{1}.{2}", moduleName, typeName, propertyName);
            else
                return string.Format("[{0}]{1}.{2}.{3}", moduleName, @namespace, typeName, propertyName);
        }

        public static bool operator ==(PropertyObfuscateItem a, PropertyObfuscateItem b)
        {
            return a.Equals(b);
        }


        public static bool operator !=(PropertyObfuscateItem a, PropertyObfuscateItem b)
        {
            return !(a == b);
        }
    }

    internal class FieldObfuscateItem : BaseObfuscateItem
    {
        private string @namespace;
        private string typeName;
        private string fieldName;

        public FieldObfuscateItem(FieldDefinition fieldDefinition)
        {
            this.moduleName = fieldDefinition.Module.Name;
            this.@namespace = fieldDefinition.DeclaringType.Namespace;
            this.typeName = fieldDefinition.DeclaringType.Name;
            this.fieldName = fieldDefinition.Name;
        }

        public FieldObfuscateItem(string @namespace, string typeName, string fieldName, string moduleName = "")
        {
            this.@namespace = @namespace;
            this.moduleName = moduleName;
            this.typeName = typeName;
            this.fieldName = fieldName;
        }

        public override string Name
        {
            get
            {
                return fieldName;
            }
        }

        public override int GetHashCode()
        {
            return @namespace.GetHashCode() ^ typeName.GetHashCode() ^ fieldName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is FieldObfuscateItem && Equals((FieldObfuscateItem)obj);
        }

        public override bool Equals(BaseObfuscateItem other)
        {
            return other is FieldObfuscateItem && Equals((FieldObfuscateItem)other);
        }

        public bool Equals(FieldObfuscateItem other)
        {
            return @namespace == other.@namespace && typeName == other.typeName && fieldName == other.fieldName;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(@namespace))
                return string.Format("[{0}]{1}.{2}", moduleName, typeName, fieldName);
            else
                return string.Format("[{0}]{1}.{2}.{3}", moduleName, @namespace, typeName, fieldName);
        }

        public static bool operator ==(FieldObfuscateItem a, FieldObfuscateItem b)
        {
            return a.Equals(b);
        }


        public static bool operator !=(FieldObfuscateItem a, FieldObfuscateItem b)
        {
            return !(a == b);
        }
    }
}


