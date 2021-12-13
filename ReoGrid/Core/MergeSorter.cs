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
using System.Linq;

namespace unvell.ReoGrid.Core
{
    /// <summary>
    /// An implement for merge sort algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MergeSorter<T>
    {
        private readonly Func<T, T, int> compare;
        private readonly SortOrder order;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="compare">compare function for array element</param>
        public MergeSorter(Func<T, T, int> compare, SortOrder order)
        {
            this.compare = compare;
            this.order = order;
        }

        public T[] Sort(T[] arrayToSort)
        {
            var arr = new T[arrayToSort.Length];

            Array.Copy(arrayToSort, arr, arrayToSort.Length);

            if (arr.Length < 2) 
                return arr;

            var middle = arr.Length / 2;

            var left = new T[middle];
            Array.Copy(arrayToSort, left, middle);
            var right = new T[arrayToSort.Length - middle];
            Array.Copy(arrayToSort, middle, right, 0, right.Length);

            return Merge(Sort(left), Sort(right));
        }

        private T[] Merge(T[] left, T[] right)
        {
            var result = new T[left.Length + right.Length];
            var i = 0;
            while (left.Length > 0 && right.Length > 0) 
            {
                if (Compare(left[0], right[0])) 
                {
                    result[i++] = left[0];
                    left = left.Skip(1).Take(left.Length - 1).ToArray();
                }
                else 
                {
                    result[i++] = right[0];
                    right = right.Skip(1).Take(right.Length - 1).ToArray();;
                }
            }

            if (left.Length > 0)
                Array.Copy(left, 0, result, i++, left.Length);

            if (right.Length > 0)
                Array.Copy(right, 0, result, i++, right.Length);

            return result;
        }

        private bool Compare(T left, T right)
        {
            return order == SortOrder.Ascending
                ? compare(left, right) <= 0
                : compare(left, right) >= 0;
        }
    }
}