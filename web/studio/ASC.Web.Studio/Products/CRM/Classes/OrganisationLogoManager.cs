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

#region Import

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using System.Linq;
using ASC.Collections;
using ASC.Common.Threading.Workers;
using ASC.Data.Storage;
using ASC.Web.CRM.Configuration;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Core;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using ASC.Web.CRM.Resources;

#endregion

namespace ASC.Web.CRM.Classes
{
    public static class OrganisationLogoManager
    {
        #region Members

        public static readonly String OrganisationLogoBaseDirName = "organisationlogo";
        public static readonly String OrganisationLogoImgName = "logo";

        public static readonly String OrganisationLogoSrcFormat = "data:image/jpeg;base64,{0}";

        public static readonly Size OrganisationLogoSize = new Size(200, 150);

        private static readonly Object _synchronizedObj = new Object();

        #endregion

        #region DataStore Methods

        private static String FromDataStore()
        {
            var directoryPath = BuildFileDirectory();
            var filesURI = Global.GetStore().ListFiles(directoryPath, OrganisationLogoImgName + "*", false);

            if (filesURI.Length == 0)
            {
                return String.Empty;
            }
            return filesURI[0].ToString();
        }

        #endregion

        #region Private Methods

        private static String BuildFileDirectory()
        {
            return String.Concat(OrganisationLogoBaseDirName, "/");
        }

        private static String BuildFilePath(String imageExtension)
        {
            return String.Concat(BuildFileDirectory(), OrganisationLogoImgName, imageExtension);
        }


        private static String ExecResizeImage(byte[] imageData, Size fotoSize, IDataStore dataStore, String photoPath)
        {
            var data = imageData;
            using (var stream = new MemoryStream(data))
            using (var img = new Bitmap(stream))
            {
                var imgFormat = img.RawFormat;
                if (fotoSize != img.Size)
                {
                    using (var img2 = Global.DoThumbnail(img, fotoSize))
                    {
                        data = Global.SaveToBytes(img2, imgFormat);
                    }
                }
                else
                {
                    data = Global.SaveToBytes(img);
                }

                var fileExtension = String.Concat("." + Global.GetImgFormatName(ImageFormat.Jpeg));

                using (var fileStream = new MemoryStream(data))
                {
                    var photoUri = dataStore.Save(photoPath, fileStream).ToString();
                    photoUri = String.Format("{0}?cd={1}", photoUri, DateTime.UtcNow.Ticks);
                    return photoUri;
                }
            }
        }

      

        #endregion

        public static String GetDefaultLogoUrl()
        {
            return WebImageSupplier.GetAbsoluteWebPath("org_logo_default.png", ProductEntryPoint.ID);
        }

        public static String GetOrganisationLogoBase64(int logoID)
        {
            if (logoID <= 0) { return ""; }
            return Global.DaoFactory.GetInvoiceDao().GetOrganisationLogoBase64(logoID);
        }

        public static String GetOrganisationLogoSrc(int logoID)
        {
            var bytestring = GetOrganisationLogoBase64(logoID);
            return String.IsNullOrEmpty(bytestring) ? "" : String.Format(OrganisationLogoSrcFormat, bytestring);
        }

        public static void DeletePhoto(bool recursive)
        {
            var photoDirectory = BuildFileDirectory();
            var store = Global.GetStore();

            lock (_synchronizedObj)
            {
                if (store.IsDirectory(photoDirectory))
                {
                    store.DeleteFiles(photoDirectory, "*", recursive);
                    if (recursive)
                    {
                        store.DeleteDirectory(photoDirectory);
                    }
                }
            }
        }

        public static int TryUploadOrganisationLogoFromTmp()
        {
            var logo_id = 0;
            var directoryPath = BuildFileDirectory();
            var dataStore = Global.GetStore();

            if (!dataStore.IsDirectory(directoryPath))
                return 0;

            try
            {
                var photoPath = FromDataStore();
                photoPath = photoPath.Substring(photoPath.IndexOf(directoryPath));
                var imageExtension = photoPath.Substring(photoPath.LastIndexOf("."));

                byte[] bytes;
                using (var photoTmpStream = dataStore.GetReadStream(photoPath))
                {
                    bytes = Global.ToByteArray(photoTmpStream);
                }

                logo_id = Global.DaoFactory.GetInvoiceDao().SaveOrganisationLogo(bytes);
                dataStore.DeleteFiles(directoryPath, "*", false);
                return logo_id;
            }

            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("ASC.CRM").ErrorFormat("TryUploadOrganisationLogoFromTmp failed with error: {0}", ex);
                return 0;
            }
        }

        public static String UploadLogo(Stream inputStream, bool isTmpDir)
        {
            var imageData = Global.ToByteArray(inputStream);

            var fileExtension = String.Concat("." + Global.GetImgFormatName(ImageFormat.Jpeg));
            var photoPath = BuildFilePath(fileExtension);

            var result = ExecResizeImage(imageData, OrganisationLogoSize, Global.GetStore(), photoPath);

            return result;
        }
    }
}