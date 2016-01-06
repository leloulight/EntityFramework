// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public interface IShaper<out T>
    {
        T Shape([NotNull] QueryContext queryContext, ValueBuffer valueBuffer);

        bool IsShaperForQuerySource([NotNull] IQuerySource querySource);
    }
}
