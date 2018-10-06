using System;
using System.Collections.Generic;
using System.Text;

namespace SkyEditor.Core.IO
{
    public interface IBinaryDataAccessor : IReadOnlyBinaryDataAccessor, IWriteOnlyBinaryDataAccessor
    {
    }
}
