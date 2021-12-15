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

namespace unvell.ReoGrid.Data
{
    public delegate void OnColumnSortedEventHandler(object sender, OnColumnSortedEventHandlerArgs args);

    /// <summary>
    /// Args class for OnColumnSortedEvent
    /// </summary>
    public class OnColumnSortedEventHandlerArgs : EventArgs
    {
        /// <summary>
        /// The range that column sorting affected
        /// </summary>
        public RangePosition Range { get; }

        /// <inheritdoc />
        public OnColumnSortedEventHandlerArgs(RangePosition range) : base()
        {
            this.Range = range;
        }
    }
}