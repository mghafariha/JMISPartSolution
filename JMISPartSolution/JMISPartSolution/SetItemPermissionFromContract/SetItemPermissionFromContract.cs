using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.Collections.Generic;

namespace JMISPartSolution.SetItemPermissionFromContract
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class SetItemPermissionFromContract : SPItemEventReceiver
{
    // Methods
    public void GetRelatedUser(SPWeb jmisWeb, int contractID, out int contractorId, out int advisorId, out int managerId, out int areaManagerId, out List<int> prReaders)
    {
        SPList list = jmisWeb.GetList("sites/jmis/Lists/Contracts");
        SPList list2 = jmisWeb.GetList("sites/jmis/Lists/Areas");
        SPList list3 = jmisWeb.GetList("sites/jmis/Lists/FormPermissions");
        SPListItem itemById = list.GetItemById(contractID);
        contractorId = new SPFieldLookupValue(itemById["ContractorUser"].ToString()).LookupId;
        advisorId = new SPFieldLookupValue(itemById["ConsultentUser"].ToString()).LookupId;
        managerId = new SPFieldLookupValue(itemById["ManagerUser"].ToString()).LookupId;
        SPListItem item2 = list2.GetItemById(new SPFieldLookupValue(itemById["Area"].ToString()).LookupId);
        areaManagerId = new SPFieldLookupValue(item2["AreaManagerUser"].ToString()).LookupId;
        prReaders = new List<int>();
        SPFieldUserValueCollection values = (itemById["Viewers"] != null) ? new SPFieldUserValueCollection(jmisWeb, itemById["Viewers"].ToString()) : null;
        if (values != null)
        {
            foreach (SPFieldUserValue value2 in values)
            {
                int lookupId = value2.LookupId;
                prReaders.Add(lookupId);
            }
        }
    }

    public override void ItemAdded(SPItemEventProperties properties)
    {
        int lineNumber = 0;
        base.ItemAdded(properties);
        try
        {
            string siteURL = properties.Web.Url;
            Guid listId = properties.ListId;
            SPListItem item = properties.ListItem;
            int iD = item.ID;
            SPFileLevel level = item.File.Level;
            SPSecurity.RunWithElevatedPrivileges(delegate {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        lineNumber = 1;
                        SPWeb web2 = web.Site.WebApplication.Sites["jmis"].OpenWeb();
                        SPList list = web.GetList("sites/jmis/Lists/FormPermissions");
                        SPList list2 = web.Lists[listId];
                        string url = list2.RootFolder.Url;
                        string str3 = url.Substring(url.LastIndexOf("/") + 1);
                        SPQuery query = new SPQuery {
                            Query = string.Format("<Where>\r\n                                                              <Eq>\r\n                                                                 <FieldRef Name='ListName' />\r\n                                                                 <Value Type='Text'>{0}</Value>\r\n                                                              </Eq>\r\n                                                           </Where>", str3)
                        };
                        lineNumber = 2;
                        if (list.GetItems(query).Count > 0)
                        {
                            int lookupId;
                            SPListItem item1 = list.GetItems(query)[0];
                            lineNumber = 3;
                            string str4 = item1["PermissionField"].ToString();
                            string str5 = (item1["PermissionLookupList"] != null) ? item1["PermissionLookupList"].ToString() : "";
                            string str6 = (item1["PermissionLookupListField"] != null) ? item1["PermissionLookupListField"].ToString() : "";
                            SPFieldLookupValueCollection values = (item1["Editors"] != null) ? new SPFieldLookupValueCollection(item1["Editors"].ToString()) : null;
                            SPFieldLookupValueCollection values2 = (item1["Viewers"] != null) ? new SPFieldLookupValueCollection(item1["Viewers"].ToString()) : null;
                            List<int> list3 = new List<int>();
                            List<int> list4 = new List<int>();
                            if (values2 != null)
                            {
                                foreach (SPFieldUserValue value2 in values2)
                                {
                                    lineNumber = 4;
                                    lookupId = value2.LookupId;
                                    list3.Add(lookupId);
                                }
                            }
                            if (values != null)
                            {
                                foreach (SPFieldUserValue value2 in values)
                                {
                                    lineNumber = 5;
                                    lookupId = value2.LookupId;
                                    list4.Add(lookupId);
                                }
                            }
                            int contractID = 0;
                            if (str5 != "")
                            {
                                lineNumber = 6;
                                SPList list5 = web.GetList("sites/jmis/" + str5);
                                int id = new SPFieldLookupValue(item[str6].ToString()).LookupId;
                                contractID = new SPFieldLookupValue(list5.GetItemById(id)[str4].ToString()).LookupId;
                            }
                            else
                            {
                                contractID = new SPFieldLookupValue(item[str4].ToString()).LookupId;
                            }
                            lineNumber = 7;
                            int contractorId = 0;
                            int advisorId = 0;
                            int managerId = 0;
                            int areaManagerId = 0;
                            List<int> prReaders = new List<int>();
                            List<int> list7 = new List<int>();
                            this.GetRelatedUser(web, contractID, out contractorId, out advisorId, out managerId, out areaManagerId, out prReaders);
                            lineNumber = 8;
                            SetListItemPermission(item, contractorId, 0x40000002, true);
                            SetListItemPermission(item, advisorId, 0x40000002, false);
                            SetListItemPermission(item, managerId, 0x40000002, false);
                            SetListItemPermission(item, areaManagerId, 0x40000002, false);
                            SetListItemPermission(item, 8, 0x40000002, false);
                            SetListItemPermission(item, 9, 0x40000003, false);
                            foreach (int num8 in list3)
                            {
                                SetListItemPermission(item, num8, 0x40000002, false);
                                lineNumber = 9;
                            }
                            foreach (int num8 in prReaders)
                            {
                                lineNumber = 10;
                                SetListItemPermission(item, num8, 0x40000002, false);
                            }
                            foreach (int num9 in list4)
                            {
                                SetListItemPermission(item, num9, 0x40000003, false);
                            }
                            lineNumber = 11;
                        }
                    }
                }
            });
        }
        catch (Exception exception)
        {
            SPListItem listItem = properties.ListItem;
            listItem["Message"] = "اتمام ناموفق" + exception.Message + "--" + lineNumber.ToString();
            properties.Web.AllowUnsafeUpdates = true;
            listItem.Update();
        }
    }

    public static string SetListItemPermission(SPListItem Item, int userId, int PermissionID, bool ClearPreviousPermissions)
    {
        string strError = "";
        string siteURL = Item.ParentList.ParentWeb.Url;
        Guid listId = Item.ParentList.ID;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPPrincipal byID;
                    Exception exception;
                    web.AllowUnsafeUpdates = true;
                    SPListItem itemById = web.Lists[listId].GetItemById(Item.ID);
                    if (!itemById.HasUniqueRoleAssignments)
                    {
                        itemById.BreakRoleInheritance(!ClearPreviousPermissions);
                    }
                    try
                    {
                        byID = web.SiteUsers.GetByID(userId);
                    }
                    catch (Exception exception1)
                    {
                        exception = exception1;
                        byID = web.SiteGroups.GetByID(userId);
                    }
                    SPRoleAssignment roleAssignment = new SPRoleAssignment(byID);
                    SPRoleDefinition roleDefinition = web.RoleDefinitions.GetById(PermissionID);
                    roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                    itemById.RoleAssignments.Remove(byID);
                    itemById.RoleAssignments.Add(roleAssignment);
                    try
                    {
                        itemById.SystemUpdate(false);
                    }
                    catch (Exception exception2)
                    {
                        exception = exception2;
                        strError = exception.Message;
                    }
                }
            }
        });
        return strError;
    }
}

 

 

}