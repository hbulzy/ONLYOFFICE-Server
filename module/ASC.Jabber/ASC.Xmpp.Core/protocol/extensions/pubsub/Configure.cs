/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

#region using

using ASC.Xmpp.Core.protocol.x.data;

#endregion

namespace ASC.Xmpp.Core.protocol.extensions.pubsub
{
    public class Configure : PubSubAction
    {
        #region << Constructors >>

        public Configure()
        {
            TagName = "configure";
        }

        public Configure(string node) : this()
        {
            Node = node;
        }

        public Configure(Type type) : this()
        {
            Type = type;
        }

        public Configure(string node, Type type) : this(node)
        {
            Type = type;
        }

        #endregion

        public Access Access
        {
            get { return (Access) GetAttributeEnum("access", typeof (Access)); }
            set
            {
                if (value == Access.NONE)
                    RemoveAttribute("access");
                else
                    SetAttribute("access", value.ToString());
            }
        }

        /// <summary>
        ///   The x-Data Element
        /// </summary>
        public Data Data
        {
            get { return SelectSingleElement(typeof (Data)) as Data; }
            set
            {
                if (HasTag(typeof (Data)))
                    RemoveTag(typeof (Data));

                if (value != null)
                    AddChild(value);
            }
        }
    }
}