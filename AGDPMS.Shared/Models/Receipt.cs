using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGDPMS.Shared.Models;

public class Receipt
{
    string don_vi;
    string bo_phan;
    string so;
    string no;
    string co;
    string ho_ten_nguoi_giao;
    string theo;
    string cua;
    string nhap_tai_kho;
    string dia_diem;
    DateOnly ngay_thang_nam;
    IEnumerable<StockTransaction> transactions;
    string so_chung_tu_goc_kem_theo;
}