using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BusinessMenuTypes
{
    public BusinessMenuTypes()
    {
    }
    public List<BusinessMenu> BusinessMenuList { get; private set; } = new List<BusinessMenu>();
    public List<PropertyMenu> PropertyMenuList { get; private set; } = new List<PropertyMenu>();

}

