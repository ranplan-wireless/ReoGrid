//  ****************************************************************************
//  Ranplan Wireless Network Design Ltd.
//  __________________
//   All Rights Reserved. [2021]
// 
//  NOTICE:
//  All information contained herein is, and remains the property of
//  Ranplan Wireless Network Design Ltd. and its suppliers, if any.
//  The intellectual and technical concepts contained herein are proprietary
//  to Ranplan Wireless Network Design Ltd. and its suppliers and may be
//  covered by U.S. and Foreign Patents, patents in process, and are protected
//  by trade secret or copyright law.
//  Dissemination of this information or reproduction of this material
//  is strictly forbidden unless prior written permission is obtained
//  from Ranplan Wireless Network Design Ltd.
// ****************************************************************************

using System;

namespace unvell.ReoGrid.Core
{
    internal interface ISortedEntity
    {
        int OriginalIndex { get; }
        IComparable ComparableValue { get; }
    }

    /// <summary>
    /// Struct entity for cell sorting
    /// </summary>
    internal readonly struct SortedEntity : ISortedEntity
    {
        public SortedEntity(int originalIndex, IComparable cellInnerContent)
        {
            OriginalIndex = originalIndex;
            CellInnerContent = cellInnerContent;
        }

        public int OriginalIndex { get; }
        public IComparable CellInnerContent { get; }

        public IComparable ComparableValue => CellInnerContent;
    }
}