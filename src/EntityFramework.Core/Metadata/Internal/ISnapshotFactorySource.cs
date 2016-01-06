// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public interface ISnapshotFactorySource
    {
        Func<InternalEntityEntry, ISnapshot> OriginalValuesFactory { get; }
        Func<InternalEntityEntry, ISnapshot> RelationshipSnapshotFactory { get; }
        Func<ValueBuffer, ISnapshot> ShadowValuesFactory { get; }
        Func<ISnapshot> EmptyShadowValuesFactory { get; }
    }
}
