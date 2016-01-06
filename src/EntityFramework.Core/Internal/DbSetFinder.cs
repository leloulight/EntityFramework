// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Internal
{
    public class DbSetFinder : IDbSetFinder
    {
        private readonly ConcurrentDictionary<Type, IReadOnlyList<DbSetProperty>> _cache
            = new ConcurrentDictionary<Type, IReadOnlyList<DbSetProperty>>();

        public virtual IReadOnlyList<DbSetProperty> FindSets(DbContext context) => _cache.GetOrAdd(context.GetType(), FindSets);

        private static DbSetProperty[] FindSets(Type contextType)
        {
            var factory = new ClrPropertySetterFactory();

            return contextType.GetRuntimeProperties()
                .Where(
                    p => !p.IsStatic()
                         && !p.GetIndexParameters().Any()
                         && (p.DeclaringType != typeof(DbContext))
                         && p.PropertyType.GetTypeInfo().IsGenericType
                         && (p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)))
                .OrderBy(p => p.Name)
                .Select(p =>
                    {
                        return new DbSetProperty(
                            p.Name,
                            p.PropertyType.GetTypeInfo().GenericTypeArguments.Single(),
                            p.SetMethod == null ? null : factory.Create(p));
                    })
                .ToArray();
        }
    }
}
