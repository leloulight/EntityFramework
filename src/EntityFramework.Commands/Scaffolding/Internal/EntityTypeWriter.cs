﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Scaffolding.Internal.Configuration;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class EntityTypeWriter
    {
        private ScaffoldingUtilities ScaffoldingUtilities { get; }
        private CSharpUtilities CSharpUtilities { get; }
        private IndentedStringBuilder _sb;
        private EntityConfiguration _entity;

        public EntityTypeWriter(
            [NotNull] ScaffoldingUtilities scaffoldingUtilities,
            [NotNull] CSharpUtilities cSharpUtilities)
        {
            Check.NotNull(scaffoldingUtilities, nameof(scaffoldingUtilities));
            Check.NotNull(cSharpUtilities, nameof(cSharpUtilities));

            ScaffoldingUtilities = scaffoldingUtilities;
            CSharpUtilities = cSharpUtilities;
        }

        public virtual string WriteCode(
            [NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));

            _entity = entityConfiguration;
            _sb = new IndentedStringBuilder();

            _sb.AppendLine("using System;");
            _sb.AppendLine("using System.Collections.Generic;");
            if (!_entity.ModelConfiguration.CustomConfiguration.UseFluentApiOnly)
            {
                _sb.AppendLine("using System.ComponentModel.DataAnnotations;");
                _sb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            }

            foreach (var ns in _entity.EntityType.GetProperties()
                .Select(p => p.ClrType.Namespace)
                .Where(ns => ns != "System" && ns != "System.Collections.Generic")
                .Distinct())
            {
                _sb
                    .Append("using ")
                    .Append(ns)
                    .AppendLine(';');
            }

            _sb.AppendLine();
            _sb.AppendLine("namespace " + _entity.ModelConfiguration.Namespace());
            _sb.AppendLine("{");
            using (_sb.Indent())
            {
                AddClass();
            }
            _sb.AppendLine("}");

            return _sb.ToString();
        }

        public virtual void AddClass()
        {
            AddAttributes(_entity.AttributeConfigurations);
            _sb.AppendLine("public partial class " + _entity.EntityType.Name);
            _sb.AppendLine("{");
            using (_sb.Indent())
            {
                AddConstructor();
                AddProperties();
                AddNavigationProperties();
            }
            _sb.AppendLine("}");
        }

        public virtual void AddConstructor()
        {
            if (_entity.NavigationPropertyInitializerConfigurations.Count > 0)
            {
                _sb.AppendLine("public " + _entity.EntityType.Name + "()");
                _sb.AppendLine("{");
                using (_sb.Indent())
                {
                    foreach (var navPropInitializer in _entity.NavigationPropertyInitializerConfigurations)
                    {
                        _sb.AppendLine(
                            navPropInitializer.NavigationPropertyName
                            + " = new HashSet<"
                            + navPropInitializer.PrincipalEntityTypeName + ">();");
                    }
                }
                _sb.AppendLine("}");
                _sb.AppendLine();
            }
        }

        public virtual void AddProperties()
        {
            foreach (var property in ScaffoldingUtilities.OrderedProperties(_entity.EntityType))
            {
                PropertyConfiguration propertyConfiguration = _entity.FindPropertyConfiguration(property);
                if (propertyConfiguration != null)
                {
                    AddAttributes(propertyConfiguration.AttributeConfigurations);
                }

                _sb.AppendLine("public "
                    + CSharpUtilities.GetTypeName(property.ClrType)
                    + " " + property.Name + " { get; set; }");
            }
        }

        public virtual void AddNavigationProperties()
        {
            if (_entity.NavigationPropertyConfigurations.Count > 0)
            {
                _sb.AppendLine();
                foreach (var navProp in _entity.NavigationPropertyConfigurations)
                {
                    if (navProp.ErrorAnnotation != null)
                    {
                        _sb.AppendLine("// " + navProp.ErrorAnnotation);
                    }
                    else
                    {
                        AddAttributes(navProp.AttributeConfigurations);
                        _sb.AppendLine("public virtual " + navProp.Type
                            + " " + navProp.Name + " { get; set; }");
                    }
                }
            }
        }

        public virtual void AddAttributes(
            [NotNull] IEnumerable<IAttributeConfiguration> attributeConfigurations)
        {
            Check.NotNull(attributeConfigurations, nameof(attributeConfigurations));

            if (!_entity.ModelConfiguration.CustomConfiguration.UseFluentApiOnly)
            {
                foreach (var attrConfig in attributeConfigurations)
                {
                    _sb.AppendLine("[" + attrConfig.AttributeBody + "]");
                }
            }
        }
    }
}
