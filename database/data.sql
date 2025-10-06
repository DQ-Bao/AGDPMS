set client_encoding to 'utf8';

insert into roles ("name") values ('Admin'), ('Technician');

insert into users ("fullname", "phone", "password_hash", "role_id")
values ('Doãn Quốc Bảo', '0382633428', 'AQAAAAIAAYagAAAAEC7iGEcwGcYC51eb2ijKCRyIa18U40iGykiY27MJ06+6UzKwx/heauSLbMSeFifZag==', 1);

insert into material ("id", "name", "weight", "type")
values
-- Cửa đi mở quay
('C3328', N'Khung cửa đi', 1.257, 'aluminum'),
('C3303', N'Cánh cửa đi mở ngoài (có gân)', 1.441, 'aluminum'),
('C18772', N'Cánh cửa đi mở ngoài (không gân)', 1.431, 'aluminum'),
('C3332', N'Cánh cửa đi mở trong (có gân)', 1.442, 'aluminum'),
('C18782', N'Cánh cửa đi mở trong (không gân)', 1.431, 'aluminum'),

('C3322', N'Cánh cửa đi mở ngoài (có gân & bo cạnh)', 1.507, 'aluminum'),
('C22912', N'Cánh cửa đi mở ngoài (không gân & bo cạnh)', 1.496, 'aluminum'),
('C38032', N'Cánh cửa đi ngang dưới (có gân)', 2.260, 'aluminum'),
('C3304', N'Cánh cửa đi ngang dưới (có gân)', 2.023, 'aluminum'),
('C6614', N'Cánh cửa đi ngang dưới (không gân)', 2.014, 'aluminum'),

('C3323', N'Đố động cửa đi', 1.086, 'aluminum'),
('C22903', N'Đố động cửa đi và cửa sổ', 0.891, 'aluminum'),
('C22900', N'Ốp đáy cánh cửa đi', 0.476, 'aluminum'),
('C3329', N'Ốp đáy cánh cửa đi', 0.428, 'aluminum'),
('C3319', N'Ngưỡng cửa đi', 0.689, 'aluminum'),

('C3291', N'Nẹp kính', 0.206, 'aluminum'),
('C3225', N'Nẹp kính', 0.211, 'aluminum'),
('C3296', N'Nẹp kính', 0.237, 'aluminum'),
('F347', N'Ke góc', 4.957, 'aluminum'),

('C3246', N'Nẹp kính', 0.216, 'aluminum'),
('C3286', N'Nẹp kính', 0.223, 'aluminum'),
('C3236', N'Nẹp kính', 0.227, 'aluminum'),
('C3206', N'Nẹp kính', 0.257, 'aluminum'),
('C3295', N'Nẹp kính', 0.271, 'aluminum'),

-- Cửa sổ mở quay
('C3318', N'Khung cửa sổ',  0.876, 'aluminum'),
('C8092', N'Cánh cửa sổ mở ngoài (có gân)', 1.064, 'aluminum'),
('C3202', N'Cánh cửa sổ mở ngoài (có gân)', 1.088, 'aluminum'),
('C18762', N'Cánh cửa sổ mở ngoài (không gân)', 1.081, 'aluminum'),
('C3312', N'Cánh cửa sổ mở ngoài (có gân & bo cạnh)', 1.159, 'aluminum'),

('C22922', N'Cánh cửa sổ mở ngoài (không gân & bo cạnh', 1.118, 'aluminum'),
('C3033', N'Đố động cửa sổ', 0.825, 'aluminum'),
--('C22903', N'Đố động cửa đi và cửa sổ', 0.891, 'aluminum'), --duplicate
('C3313', N'Đố cố định trên khung', 1.126, 'aluminum'),
('C3209', N'Khung vách kính', 0.876, 'aluminum'),

('C3203', N'Đố cố định (có lỗ vít)', 0.314, 'aluminum'),
('F077', N'Pano', 0.664, 'aluminum'),
('E1283', N'Khung lá sách', 0.290, 'aluminum'),
('E192', N'Lá sách', 0.317, 'aluminum'),
('B507', N'Nan dán trang trí', 0.150, 'aluminum'),

('C3300', N'Nối khung', 0.347, 'aluminum'),
('C3310', N'Nối khung', 1.308, 'aluminum'),
('C3210', N'Nối khung 90 độ (bo cạnh)', 1.275, 'aluminum'),
('C920', N'Nối khung 90 độ (vuông cạnh)', 1.126, 'aluminum'),
('C910', N'Nối khung 135 độ', 0.916, 'aluminum'),

('C459', N'Thanh truyền khóa', 0.139, 'aluminum'),

('C3317', N'Pát liên kết (đố cố định với Fix)', 1.105, 'aluminum'),
('C3207', N'Pát liên kết (đố cố định với Fix)', 1.154, 'aluminum'),
('C1687', N'Ke góc', 3.134, 'aluminum'),
('C4137', N'Ke góc', 1.879, 'aluminum'),
('C1697', N'Ke góc', 2.436, 'aluminum'),

--MẶT CẮT THANH NHÔM CỬA ĐI VÀ CỬA SỔ MỞ QUAY
('C38019', 'Khung cửa đi bản 100', 2.057, 'aluminum'),
('C38038', 'Khung cửa sổ bản 100', 1.421, 'aluminum'),
('C38039', 'Khung vách kính bản 100 (loại 1 nẹp)', 1.375, 'aluminum'),
('C48949', 'Khung vách kính bản 100 (loại 2 nẹp)', 1.272, 'aluminum'),

('C48954', 'Đố cố định bản 100 (loại 1 nẹp)', 1.521, 'aluminum'),
('C48953', 'Đố cố định bản 100 (loại 2 nẹp)', 1.405, 'aluminum'),
('C38010', 'Nối khung bản 100', 0.617, 'aluminum'),
('C48980', 'Nối khung 90 độ bản 100', 2.090, 'aluminum'),

('C48945', 'Nẹp phụ bản 100', 0.346, 'aluminum'),
--('F347', 'Ke góc', 4.957, 'aluminum'), --duplicate
--('C1687', 'Ke góc', 3.134, 'aluminum'), --duplicate
--('C4137', 'Ke góc', 1.879, 'aluminum'), --duplicate

--MẶT CẮT THANH NHÔM CỬA ĐI MỞ QUAY
('CX283', 'Khung cửa đi', 1.533, 'aluminum'),
('CX281', 'Cánh cửa đi mở ngoài', 1.839, 'aluminum'),
('CX282', 'Cánh ngang dưới cửa đi', 3.033, 'aluminum'),
('CX568', 'Đố động cửa đi', 1.195, 'aluminum'),
('CX309', 'Nối khung', 0.427, 'aluminum'),

--('C22900', 'Ốp đáy cánh cửa đi', 0.476, 'aluminum'), --duplicate
--('C3329', 'Ốp đáy cánh cửa đi', 0.428, 'aluminum'), --duplicate
--('C3319', 'Ngưỡng cửa đi', 0.689, 'aluminum'), --duplicate
--('C459', 'Thanh truyền khóa', 0.139, 'aluminum'), --duplicate
--('B507', 'Nan dán trang trí', 0.150, 'aluminum'), --duplicate

--('F347', 'Ke góc', 4.957, 'aluminum'), --duplicate

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ QUAY
('CX267', 'Khung cửa sổ, vách kính', 1.057, 'aluminum'),
('CX264', 'Cánh cửa sổ mở ngoài', 1.419, 'aluminum'),
('CX750', 'Đố động cửa sổ', 0.985, 'aluminum'),
('CX266', 'Đố cố định (có lỗ vít)', 1.233, 'aluminum'),
('CX265', 'Đố cố định (không lỗ vít)', 1.163, 'aluminum'),

('C25899', 'Khung bao chuyển hướng', 0.727, 'aluminum'),
('CX311', 'Nối khung vách kính', 1.461, 'aluminum'),
('CX310', 'Thanh nối góc 90 độ', 1.614, 'aluminum'),

--('C3246', 'Nẹp kính', 0.216, 'aluminum'), --duplicate
--('C3286', 'Nẹp kính', 0.223, 'aluminum'), --duplicate
--('C3236', 'Nẹp kính', 0.227, 'aluminum'), --duplicate
--('C3206', 'Nẹp kính', 0.257, 'aluminum'), --duplicate
--('C3295', 'Nẹp kính', 0.271, 'aluminum'), --duplicate

--('C1687', 'Ke góc', 3.134, 'aluminum'), --duplicate
--('C4137', 'Ke góc', 1.879, 'aluminum'), --duplicate
--('C1697', 'Ke góc', 2.436, 'aluminum'), --duplicate
('C1757', 'Ke góc', 2.167, 'aluminum'),

--MẶT CẮT THANH NHÔM CỬA ĐI VÀ CỬA SỔ MỞ QUAY
('C40988', 'Khung cửa đi và cửa sổ', 0.862, 'aluminum'),
('C48952', 'Cánh cửa đi vát cạnh bằng', 0.991, 'aluminum'),
('C40912', 'Cánh cửa đi vát cạnh lệch', 1.008, 'aluminum'),
('C48942', 'Cánh cửa sổ vát cạnh bằng', 0.908, 'aluminum'),
('C40902', 'Cánh cửa sổ vát cạnh lệch', 0.924, 'aluminum'),

('C40983', 'Đố cố định vát cạnh bằng', 0.977, 'aluminum'),
('C40984', 'Đố cố định vát cạnh lệch', 1.021, 'aluminum'),
('C44249', 'Khung vách kính', 0.753, 'aluminum'),
('C44234', 'Đố cố định (có lỗ vít)', 0.857, 'aluminum'),
('C40869', 'Đố động cửa đi và cửa sổ', 0.701, 'aluminum'),

('C40973', 'Đố cố định trên khung', 0.860, 'aluminum'),
('C40978', 'Ốp đáy cánh cửa đi', 0.375, 'aluminum'),
('E17523', 'Pano', 0.605, 'aluminum'),
('C44226', 'Nẹp kính', 0.199, 'aluminum'),
('C40979', 'Nẹp kính', 0.218, 'aluminum'),

--MẶT CẮT THANH NHÔM CỬA ĐI XẾP TRƯỢT
('F605', 'Khung ngang trên', 3.107, 'aluminum'),
('F606', 'Khung đứng', 1.027, 'aluminum'),
('F4116', 'Khung đứng (khoá đa điểm)', 1.056, 'aluminum'),
('F607', 'Khung ngang dưới (ray nổi)', 1.053, 'aluminum'),
('F2435', 'Khung ngang dưới (ray âm)', 1.351, 'aluminum'),

('F523', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1.254, 'aluminum'),
('F4117', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1.269, 'aluminum'),
('F5017', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1.307, 'aluminum'),
('F522', 'Cánh cửa có lỗ vít (khoá đơn điểm)', 1.336, 'aluminum'),
('F560', 'Đố cố định trên cánh', 1.142, 'aluminum'),

('F520', 'Ốp giữa 2 cánh mở', 0.241, 'aluminum'),
('F519', 'Ốp che nước mưa', 0.177, 'aluminum'),
('F6029', 'Nẹp kính', 0.276, 'aluminum'),
('F521', 'Nẹp kính', 0.222, 'aluminum'),

('F608', 'Ke liên kết khung đứng với ngang trên', 1.440, 'aluminum'),
('F609', 'Ke liên kết khung đứng với ngang dưới', 1.377, 'aluminum'),
('F417', 'Ke góc', 5.228, 'aluminum'),
--('F347', 'Ke góc', 4.957, 'aluminum'), --duplicate

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ LÙA
('D23151', 'Khung cửa lùa', 0.949, 'aluminum'),
('D45482', 'Khung cửa lùa 3 ray', 1.414, 'aluminum'),
('D23156', 'Cánh cửa lùa', 0.936, 'aluminum'),
('D23157', 'Ốp cánh cửa lùa', 0.365, 'aluminum'),
('D23158', 'Nẹp đối đầu cửa 4 cánh', 0.229, 'aluminum'),

('D23159', 'Ốp che nước mưa', 0.279, 'aluminum'),

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ LÙA
('D44329', 'Khung cửa lùa', 0.885, 'aluminum'),
('D44035', 'Cánh cửa mở lùa', 0.827, 'aluminum'),
('D44327', 'Ốp cánh cửa lùa', 0.315, 'aluminum'),
('D44328', 'Nẹp đối đầu cửa 4 cánh', 0.396, 'aluminum'),

--MẶT CẮT THANH NHÔM CỬA ĐI MỞ LÙA
('D47713', 'Khung cửa lùa', 1.223, 'aluminum'),
('D45316', 'Cánh cửa mở lùa', 1.319, 'aluminum'),
('D44564', 'Cánh cửa mở lùa', 1.201, 'aluminum'),
('D47688', 'Nẹp đối đầu cửa 4 cánh', 0.545, 'aluminum'),
('D46070', 'Ốp khóa đa điểm', 0.364, 'aluminum'),

('D47679', 'Ốp đậy ray', 0.096, 'aluminum'),
('D47678', 'Ốp đậy rãnh phụ kiện', 0.073, 'aluminum'),
('D45478', 'Thanh ốp móc', 0.383, 'aluminum'),
('D44569', 'Nẹp kính', 0.199, 'aluminum'),

--MẶT CẮT THANH NHÔM CỬA MỞ LÙA
('D1541A', 'Khung ngang trên', 1.459, 'aluminum'),
('D1551A', 'Đố chia cửa lùa với vách kính trên', 2.164, 'aluminum'),
('D17182', 'Khung ngang dưới (ray bằng)', 1.307, 'aluminum'),
('D1942', 'Khung ngang dưới (ray lệch)', 1.561, 'aluminum'),
('D1542A', 'Khung ngang dưới (ray lệch)', 1.706, 'aluminum'),

('D1543A', 'Khung đứng', 1.134, 'aluminum'),
('D3213', 'Khung đứng (3 ray)', 1.367, 'aluminum'),
('D3211', 'Khung ngang trên (3 ray)', 1.959, 'aluminum'),
('D3212', 'Khung ngang dưới (3 ray)', 2.295, 'aluminum'),
('D1544A', 'Cánh ngang trên', 0.990, 'aluminum'),

('D1545A', 'Cánh ngang dưới', 1.000, 'aluminum'),
('D1546A', 'Cánh đứng trơn', 1.273, 'aluminum'),
('D1547A', 'Cánh đứng móc', 1.098, 'aluminum'),
('D28144', 'Cánh ngang trên', 1.115, 'aluminum'),
('D1555A', 'Cánh ngang dưới', 1.243, 'aluminum'),

('D26146', 'Cánh đứng trơn', 1.330, 'aluminum'),
('D28127', 'Cánh đứng móc', 1.303, 'aluminum'),
('D1559A', 'Khung đứng vách kính', 1.070, 'aluminum'),
('D2618', 'Đố cố định trên vách kính', 1.546, 'aluminum'),
('D1354', 'Đố cố định trên cánh', 0.696, 'aluminum'),

('D1548A', 'Nẹp đối đầu cửa 4 cánh', 0.620, 'aluminum'),
('D1549A', 'Ốp khung vách kính', 0.712, 'aluminum'),
('D1578', 'Nối khung vách kính', 0.676, 'aluminum'),
('D2420', 'Nối góc 90 độ trái', 2.292, 'aluminum'),
('D2490', 'Nối góc 90 độ phải', 2.292, 'aluminum'),

('D34608', 'Thanh chuyển kính hộp', 0.399, 'aluminum'),
('D1779', 'Nẹp kính', 0.100, 'aluminum'),
('D1298', 'Nẹp kính', 0.109, 'aluminum'),
('D1168', 'Nẹp kính', 0.130, 'aluminum'),
('C101', 'Nẹp kính', 0.133, 'aluminum'),

--MẶT CẮT THANH NHÔM CỬA BẢN LỀ SÀN
('F631', 'Cánh đứng', 2.570, 'aluminum'),
('F632', 'Cánh ngang trên', 2.382, 'aluminum'),
('F633', 'Cánh ngang dưới', 2.382, 'aluminum'),

('F2084', 'Đố tĩnh', 2.278, 'aluminum'),
('F630', 'Nẹp kính', 0.173, 'aluminum'),
('F949', 'Nẹp kính', 0.176, 'aluminum'),

--MỘT SỐ MÃ PHỤ
('D47680', 'Ngưỡng nhôm', 0.408, 'aluminum'),
('A1079', 'Nẹp lưới chống muỗi', 0.087, 'aluminum'),
('A1080', 'Nẹp lưới chống muỗi', 0.087, 'aluminum'),
('D47590', 'Ray nhôm cho cửa nhựa', 0.040, 'aluminum'),

--MẶT CẮT THANH NHÔM MẶT DỰNG LỘ ĐỐ
('GK461', 'Thanh đố đứng', 2.138, 'aluminum'),
('GK471', 'Thanh đố đứng', 2.281, 'aluminum'),
('GK481', 'Thanh đố đứng', 2.424, 'aluminum'),
('GK491', 'Thanh đố đứng', 2.567, 'aluminum'),

('GK501', 'Thanh đố đứng', 2.711, 'aluminum'),
('E21451', 'Thanh đố đứng', 2.347, 'aluminum'),
('GK581', 'Thanh đố đứng (kính hộp)', 2.730, 'aluminum'),
('GK993', 'Thanh đố ngang', 1.908, 'aluminum'),

('GK2053', 'Thanh đố ngang', 1.863, 'aluminum'),
('GK2467', 'Thanh nêm đố ngang', 0.304, 'aluminum'),
('GK858', 'Pat liên kết thang ngang', 1.218, 'aluminum'),
('GK1073', 'Nắp đậy đố ngang', 0.292, 'aluminum'),

('GK015', 'Đế ốp mặt ngoài', 0.577, 'aluminum'),
('GK066', 'Nắp đậy đế ốp', 0.404, 'aluminum'),
('GK780', 'Nối góc 90 độ ngoài', 0.743, 'aluminum'),
('GK1495', 'Đế ốp mặt ngoài góc 90 độ', 1.110, 'aluminum'),

('GK806', 'Nắp đậy đế ốp góc 90 độ', 1.721, 'aluminum'),
('GK1035', 'Đế ốp mặt ngoài góc 135 độ', 0.743, 'aluminum'),
('GK606', 'Nắp đậy đế ốp góc 135 độ', 0.675, 'aluminum'),
('GK294', 'Nắp đậy che rãnh', 0.138, 'aluminum'),

('GK2464', 'Nắp đậy khe rãnh', 0.264, 'aluminum'),
('GK1255', 'Khung trên cửa sổ (dạng móc treo)', 0.918, 'aluminum'),
('GK1325', 'Cánh trên cửa sổ (dạng móc treo)', 0.791, 'aluminum'),
('GK1295', 'Khung cửa sổ', 0.751, 'aluminum'),

('GK1365', 'Cánh cửa sổ', 0.801, 'aluminum'),
('GK505', 'Thanh đố kính cho cánh cửa sổ', 0.959, 'aluminum'),
('GK1215', 'Ke cửa sổ', 0.959, 'aluminum'),

--THANH NHÔM MẶT DỰNG GIẤU ĐỐ
('GK001', 'Thanh đố đứng', 1.923, 'aluminum'),
('GK011', 'Thanh đố đứng', 2.211, 'aluminum'),
('GK021', 'Thanh đố đứng', 2.495, 'aluminum'),
('GK251', 'Thanh đố đứng', 2.638, 'aluminum'),

('GK261', 'Thanh đố đứng', 3.051, 'aluminum'),
('GK813', 'Thanh đố ngang', 1.733, 'aluminum'),
('GK853', 'Thanh đố ngang', 1.757, 'aluminum'),
('GK413', 'Nắp đậy thanh đố ngang', 0.217, 'aluminum'),

('GK1745', 'Pat liên kết thanh đố ngang', 1.173, 'aluminum'),
--('GK2467', 'Thanh nêm đố ngang', 0.304, 'aluminum'), --duplicate
('GK228', 'Nẹp kính trái', 0.356, 'aluminum'),
('GK238', 'Nẹp kính phải', 0.294, 'aluminum'),

('GK218', 'Nẹp kính trên', 0.437, 'aluminum'),
('GK208', 'Nẹp kính dưới', 0.383, 'aluminum'),
('GK255', 'Thanh móc treo kính', 0.436, 'aluminum'),
--('C459', 'Thanh truyền khóa', 0.139, 'aluminum'), --duplicate

('GK275', 'Thanh đố kính', 0.245, 'aluminum'),
('GK1064', 'Chống nhấc cánh', 0.257, 'aluminum'),
--('GK1255', 'Khung trên cửa sổ (dạng móc treo)', 0.918, 'aluminum'), --duplicate
--('GK1325', 'Cánh trên cửa sổ (dạng móc treo)', 0.791, 'aluminum'), --duplicate

--('GK1295', 'Khung cửa sổ', 0.751, 'aluminum'), --duplicate
--('GK1365', 'Cánh cửa sổ', 0.801, 'aluminum'), --duplicate
('GK534', 'Thanh đỡ kính cho cánh cửa sổ', 0.195, 'aluminum'),
('GK454', 'Máng che cánh cửa sổ', 0.288, 'aluminum'),

--('GK1215', 'Ke cửa sổ', 0.959, 'aluminum'), --duplicate

--MẶT CẮT PROFILE LAN CAN KÍNH
('E1214', 'Khung bao ngang trên', 1.795, 'aluminum'),
('E1215', 'Khung bao dưới', 0.976, 'aluminum'),
('E1216', 'Đố Lan Can', 1.131, 'aluminum'),
('B1735', 'Đố Lan Can', 1.347, 'aluminum'),

('E1217', 'Nối góc 90 độ', 1.453, 'aluminum'),
('E1218', 'Nắp đậy che rãnh', 0.110, 'aluminum'),

('B2831', 'Khung bao ngang trên', 1.402, 'aluminum'),
('B2832', 'Nắp đậy che rãnh', 0.177, 'aluminum'),
('B2846', 'Khung đứng', 1.081, 'aluminum'),
('B2833', 'Đố Lan Can', 1.404, 'aluminum'),

('B2834', 'Nối góc 90 độ', 1.617, 'aluminum'),
('B2835', 'Nắp đậy rãnh khung đứng', 0.109, 'aluminum'),

('B4425', 'Khung bao ngang trên', 1.453, 'aluminum'),
('B4426', 'Nẹp kính', 0.155, 'aluminum'),
('B4429', 'Khung bao đứng', 0.765, 'aluminum'),
('B4428', 'Đố lan can', 0.932, 'aluminum'),

('B4430', 'Nẹp kính', 0.153, 'aluminum'),
('B4427', 'Nối góc 90 độ', 1.197, 'aluminum'),

('B3730', 'Khung bao ngang trên', 1.128, 'aluminum'),
('B3731', 'Đố đứng', 0.920, 'aluminum'),
('B3732', 'Khung đứng', 0.689, 'aluminum'),
('B3733', 'Nẹp kính', 0.136, 'aluminum');
