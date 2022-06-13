﻿using QLBH.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBH.ViewModels.Catalog.Product
{
    public class ProductPagingVM : PagingRequestBase
    {
        public string Keyword { get; set; }
        public  List<int> CategoryIds { get; set; }
    }
}
