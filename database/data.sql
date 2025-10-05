set client_encoding to 'utf8';

insert into roles ("name") values ('Admin'), ('Technician');

insert into users ("fullname", "phone", "password_hash", "role_id")
values ('Doãn Quốc Bảo', '0382633428', 'AQAAAAIAAYagAAAAEC7iGEcwGcYC51eb2ijKCRyIa18U40iGykiY27MJ06+6UzKwx/heauSLbMSeFifZag==', 1);

insert into aluminum ("aluminum_id", "aluminum_name", "linear_density")
values
-- Cửa đi mở quay
('C3328', N'Khung cửa đi', 1.257),
('C3303', N'Cánh cửa đi mở ngoài (có gân)', 1.441),
('C18772', N'Cánh cửa đi mở ngoài (không gân)', 1.431),
('C3332', N'Cánh cửa đi mở trong (có gân)', 1.442),
('C18782', N'Cánh cửa đi mở trong (không gân)', 1.431),

('C3322', N'Cánh cửa đi mở ngoài (có gân & bo cạnh)', 1.507),
('C22912', N'Cánh cửa đi mở ngoài (không gân & bo cạnh)', 1.496),
('C38032', N'Cánh cửa đi ngang dưới (có gân)', 2.260),
('C3304', N'Cánh cửa đi ngang dưới (có gân)', 2.023),
('C6614', N'Cánh cửa đi ngang dưới (không gân)', 2.014),

('C3323', N'Đố động cửa đi', 1.086),
('C22903', N'Đố động cửa đi và cửa sổ', 0.891),
('C22900', N'Ốp đáy cánh cửa đi', 0.476),
('C3329', N'Ốp đáy cánh cửa đi', 0.428),
('C3319', N'Ngưỡng cửa đi', 0.689),

('C3291', N'Nẹp kính', 0.206),
('C3225', N'Nẹp kính', 0.211),
('C3296', N'Nẹp kính', 0.237),
('F347', N'Ke góc', 4.957),

('C3246', N'Nẹp kính', 0.216),
('C3286', N'Nẹp kính', 0.223),
('C3236', N'Nẹp kính', 0.227),
('C3206', N'Nẹp kính', 0.257),
('C3295', N'Nẹp kính', 0.271),

-- Cửa sổ mở quay
('C3318', N'Khung cửa sổ',  0.876),
('C8092', N'Cánh cửa sổ mở ngoài (có gân)', 1.064),
('C3202', N'Cánh cửa sổ mở ngoài (có gân)', 1.088),
('C18762', N'Cánh cửa sổ mở ngoài (không gân)', 1.081),
('C3312', N'Cánh cửa sổ mở ngoài (có gân & bo cạnh)', 1.159),

('C22922', N'Cánh cửa sổ mở ngoài (không gân & bo cạnh', 1.118),
('C3033', N'Đố động cửa sổ', 0.825),
--('C22903', N'Đố động cửa đi và cửa sổ', 0.891), --duplicate
('C3313', N'Đố cố định trên khung', 1.126),
('C3209', N'Khung vách kính', 0.876),

('C3203', N'Đố cố định (có lỗ vít)', 0.314),
('F077', N'Pano', 0.664),
('E1283', N'Khung lá sách', 0.290),
('E192', N'Lá sách', 0.317),
('B507', N'Nan dán trang trí', 0.150),

('C3300', N'Nối khung', 0.347),
('C3310', N'Nối khung', 1.308),
('C3210', N'Nối khung 90 độ (bo cạnh)', 1.275),
('C920', N'Nối khung 90 độ (vuông cạnh)', 1.126),
('C910', N'Nối khung 135 độ', 0.916),

('C459', N'Thanh truyền khóa', 0.139),

('C3317', N'Pát liên kết (đố cố định với Fix)', 1.105),
('C3207', N'Pát liên kết (đố cố định với Fix)', 1.154),
('C1687', N'Ke góc', 3.134),
('C4137', N'Ke góc', 1.879),
('C1697', N'Ke góc', 2.436),

--MẶT CẮT THANH NHÔM CỬA ĐI VÀ CỬA SỔ MỞ QUAY
('C38019', 'Khung cửa đi bản 100', 2.057),
('C38038', 'Khung cửa sổ bản 100', 1.421),
('C38039', 'Khung vách kính bản 100 (loại 1 nẹp)', 1.375),
('C48949', 'Khung vách kính bản 100 (loại 2 nẹp)', 1.272),

('C48954', 'Đố cố định bản 100 (loại 1 nẹp)', 1.521),
('C48953', 'Đố cố định bản 100 (loại 2 nẹp)', 1.405),
('C38010', 'Nối khung bản 100', 0.617),
('C48980', 'Nối khung 90 độ bản 100', 2.090),

('C48945', 'Nẹp phụ bản 100', 0.346),
--('F347', 'Ke góc', 4.957), --duplicate
--('C1687', 'Ke góc', 3.134), --duplicate
--('C4137', 'Ke góc', 1.879), --duplicate

--MẶT CẮT THANH NHÔM CỬA ĐI MỞ QUAY
('CX283', 'Khung cửa đi', 1.533),
('CX281', 'Cánh cửa đi mở ngoài', 1.839),
('CX282', 'Cánh ngang dưới cửa đi', 3.033),
('CX568', 'Đố động cửa đi', 1.195),
('CX309', 'Nối khung', 0.427),

--('C22900', 'Ốp đáy cánh cửa đi', 0.476), --duplicate
--('C3329', 'Ốp đáy cánh cửa đi', 0.428), --duplicate
--('C3319', 'Ngưỡng cửa đi', 0.689), --duplicate
--('C459', 'Thanh truyền khóa', 0.139), --duplicate
--('B507', 'Nan dán trang trí', 0.150), --duplicate

--('F347', 'Ke góc', 4.957), --duplicate

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ QUAY
('CX267', 'Khung cửa sổ, vách kính', 1.057),
('CX264', 'Cánh cửa sổ mở ngoài', 1.419),
('CX750', 'Đố động cửa sổ', 0.985),
('CX266', 'Đố cố định (có lỗ vít)', 1.233),
('CX265', 'Đố cố định (không lỗ vít)', 1.163),

('C25899', 'Khung bao chuyển hướng', 0.727),
('CX311', 'Nối khung vách kính', 1.461),
('CX310', 'Thanh nối góc 90 độ', 1.614),

--('C3246', 'Nẹp kính', 0.216), --duplicate
--('C3286', 'Nẹp kính', 0.223), --duplicate
--('C3236', 'Nẹp kính', 0.227), --duplicate
--('C3206', 'Nẹp kính', 0.257), --duplicate
--('C3295', 'Nẹp kính', 0.271), --duplicate

--('C1687', 'Ke góc', 3.134), --duplicate
--('C4137', 'Ke góc', 1.879), --duplicate
--('C1697', 'Ke góc', 2.436), --duplicate
('C1757', 'Ke góc', 2.167),

--MẶT CẮT THANH NHÔM CỬA ĐI VÀ CỬA SỔ MỞ QUAY
('C40988', 'Khung cửa đi và cửa sổ', 0.862),
('C48952', 'Cánh cửa đi vát cạnh bằng', 0.991),
('C40912', 'Cánh cửa đi vát cạnh lệch', 1.008),
('C48942', 'Cánh cửa sổ vát cạnh bằng', 0.908),
('C40902', 'Cánh cửa sổ vát cạnh lệch', 0.924),

('C40983', 'Đố cố định vát cạnh bằng', 0.977),
('C40984', 'Đố cố định vát cạnh lệch', 1.021),
('C44249', 'Khung vách kính', 0.753),
('C44234', 'Đố cố định (có lỗ vít)', 0.857),
('C40869', 'Đố động cửa đi và cửa sổ', 0.701),

('C40973', 'Đố cố định trên khung', 0.860),
('C40978', 'Ốp đáy cánh cửa đi', 0.375),
('E17523', 'Pano', 0.605),
('C44226', 'Nẹp kính', 0.199),
('C40979', 'Nẹp kính', 0.218),

--MẶT CẮT THANH NHÔM CỬA ĐI XẾP TRƯỢT
('F605', 'Khung ngang trên', 3.107),
('F606', 'Khung đứng', 1.027),
('F4116', 'Khung đứng (khoá đa điểm)', 1.056),
('F607', 'Khung ngang dưới (ray nổi)', 1.053),
('F2435', 'Khung ngang dưới (ray âm)', 1.351),

('F523', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1.254),
('F4117', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1.269),
('F5017', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1.307),
('F522', 'Cánh cửa có lỗ vít (khoá đơn điểm)', 1.336),
('F560', 'Đố cố định trên cánh', 1.142),

('F520', 'Ốp giữa 2 cánh mở', 0.241),
('F519', 'Ốp che nước mưa', 0.177),
('F6029', 'Nẹp kính', 0.276),
('F521', 'Nẹp kính', 0.222),

('F608', 'Ke liên kết khung đứng với ngang trên', 1.440),
('F609', 'Ke liên kết khung đứng với ngang dưới', 1.377),
('F417', 'Ke góc', 5.228),
--('F347', 'Ke góc', 4.957), --duplicate

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ LÙA
('D23151', 'Khung cửa lùa', 0.949),
('D45482', 'Khung cửa lùa 3 ray', 1.414),
('D23156', 'Cánh cửa lùa', 0.936),
('D23157', 'Ốp cánh cửa lùa', 0.365),
('D23158', 'Nẹp đối đầu cửa 4 cánh', 0.229),

('D23159', 'Ốp che nước mưa', 0.279),

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ LÙA
('D44329', 'Khung cửa lùa', 0.885),
('D44035', 'Cánh cửa mở lùa', 0.827),
('D44327', 'Ốp cánh cửa lùa', 0.315),
('D44328', 'Nẹp đối đầu cửa 4 cánh', 0.396),

--MẶT CẮT THANH NHÔM CỬA ĐI MỞ LÙA
('D47713', 'Khung cửa lùa', 1.223),
('D45316', 'Cánh cửa mở lùa', 1.319),
('D44564', 'Cánh cửa mở lùa', 1.201),
('D47688', 'Nẹp đối đầu cửa 4 cánh', 0.545),
('D46070', 'Ốp khóa đa điểm', 0.364),

('D47679', 'Ốp đậy ray', 0.096),
('D47678', 'Ốp đậy rãnh phụ kiện', 0.073),
('D45478', 'Thanh ốp móc', 0.383),
('D44569', 'Nẹp kính', 0.199),

--MẶT CẮT THANH NHÔM CỬA MỞ LÙA
('D1541A', 'Khung ngang trên', 1.459),
('D1551A', 'Đố chia cửa lùa với vách kính trên', 2.164),
('D17182', 'Khung ngang dưới (ray bằng)', 1.307),
('D1942', 'Khung ngang dưới (ray lệch)', 1.561),
('D1542A', 'Khung ngang dưới (ray lệch)', 1.706),

('D1543A', 'Khung đứng', 1.134),
('D3213', 'Khung đứng (3 ray)', 1.367),
('D3211', 'Khung ngang trên (3 ray)', 1.959),
('D3212', 'Khung ngang dưới (3 ray)', 2.295),
('D1544A', 'Cánh ngang trên', 0.990),

('D1545A', 'Cánh ngang dưới', 1.000),
('D1546A', 'Cánh đứng trơn', 1.273),
('D1547A', 'Cánh đứng móc', 1.098),
('D28144', 'Cánh ngang trên', 1.115),
('D1555A', 'Cánh ngang dưới', 1.243),

('D26146', 'Cánh đứng trơn', 1.330),
('D28127', 'Cánh đứng móc', 1.303),
('D1559A', 'Khung đứng vách kính', 1.070),
('D2618', 'Đố cố định trên vách kính', 1.546),
('D1354', 'Đố cố định trên cánh', 0.696),

('D1548A', 'Nẹp đối đầu cửa 4 cánh', 0.620),
('D1549A', 'Ốp khung vách kính', 0.712),
('D1578', 'Nối khung vách kính', 0.676),
('D2420', 'Nối góc 90 độ trái', 2.292),
('D2490', 'Nối góc 90 độ phải', 2.292),

('D34608', 'Thanh chuyển kính hộp', 0.399),
('D1779', 'Nẹp kính', 0.100),
('D1298', 'Nẹp kính', 0.109),
('D1168', 'Nẹp kính', 0.130),
('C101', 'Nẹp kính', 0.133),

--MẶT CẮT THANH NHÔM CỬA BẢN LỀ SÀN
('F631', 'Cánh đứng', 2.570),
('F632', 'Cánh ngang trên', 2.382),
('F633', 'Cánh ngang dưới', 2.382),

('F2084', 'Đố tĩnh', 2.278),
('F630', 'Nẹp kính', 0.173),
('F949', 'Nẹp kính', 0.176),

--MỘT SỐ MÃ PHỤ
('D47680', 'Ngưỡng nhôm', 0.408),
('A1079', 'Nẹp lưới chống muỗi', 0.087),
('A1080', 'Nẹp lưới chống muỗi', 0.087),
('D47590', 'Ray nhôm cho cửa nhựa', 0.040),

--MẶT CẮT THANH NHÔM MẶT DỰNG LỘ ĐỐ
('GK461', 'Thanh đố đứng', 2.138),
('GK471', 'Thanh đố đứng', 2.281),
('GK481', 'Thanh đố đứng', 2.424),
('GK491', 'Thanh đố đứng', 2.567),

('GK501', 'Thanh đố đứng', 2.711),
('E21451', 'Thanh đố đứng', 2.347),
('GK581', 'Thanh đố đứng (kính hộp)', 2.730),
('GK993', 'Thanh đố ngang', 1.908),

('GK2053', 'Thanh đố ngang', 1.863),
('GK2467', 'Thanh nêm đố ngang', 0.304),
('GK858', 'Pat liên kết thang ngang', 1.218),
('GK1073', 'Nắp đậy đố ngang', 0.292),

('GK015', 'Đế ốp mặt ngoài', 0.577),
('GK066', 'Nắp đậy đế ốp', 0.404),
('GK780', 'Nối góc 90 độ ngoài', 0.743),
('GK1495', 'Đế ốp mặt ngoài góc 90 độ', 1.110),

('GK806', 'Nắp đậy đế ốp góc 90 độ', 1.721),
('GK1035', 'Đế ốp mặt ngoài góc 135 độ', 0.743),
('GK606', 'Nắp đậy đế ốp góc 135 độ', 0.675),
('GK294', 'Nắp đậy che rãnh', 0.138),

('GK2464', 'Nắp đậy khe rãnh', 0.264),
('GK1255', 'Khung trên cửa sổ (dạng móc treo)', 0.918),
('GK1325', 'Cánh trên cửa sổ (dạng móc treo)', 0.791),
('GK1295', 'Khung cửa sổ', 0.751),

('GK1365', 'Cánh cửa sổ', 0.801),
('GK505', 'Thanh đố kính cho cánh cửa sổ', 0.959),
('GK1215', 'Ke cửa sổ', 0.959),

--THANH NHÔM MẶT DỰNG GIẤU ĐỐ
('GK001', 'Thanh đố đứng', 1.923),
('GK011', 'Thanh đố đứng', 2.211),
('GK021', 'Thanh đố đứng', 2.495),
('GK251', 'Thanh đố đứng', 2.638),

('GK261', 'Thanh đố đứng', 3.051),
('GK813', 'Thanh đố ngang', 1.733),
('GK853', 'Thanh đố ngang', 1.757),
('GK413', 'Nắp đậy thanh đố ngang', 0.217),

('GK1745', 'Pat liên kết thanh đố ngang', 1.173),
--('GK2467', 'Thanh nêm đố ngang', 0.304), --duplicate
('GK228', 'Nẹp kính trái', 0.356),
('GK238', 'Nẹp kính phải', 0.294),

('GK218', 'Nẹp kính trên', 0.437),
('GK208', 'Nẹp kính dưới', 0.383),
('GK255', 'Thanh móc treo kính', 0.436),
--('C459', 'Thanh truyền khóa', 0.139), --duplicate

('GK275', 'Thanh đố kính', 0.245),
('GK1064', 'Chống nhấc cánh', 0.257),
--('GK1255', 'Khung trên cửa sổ (dạng móc treo)', 0.918), --duplicate
--('GK1325', 'Cánh trên cửa sổ (dạng móc treo)', 0.791), --duplicate

--('GK1295', 'Khung cửa sổ', 0.751), --duplicate
--('GK1365', 'Cánh cửa sổ', 0.801), --duplicate
('GK534', 'Thanh đỡ kính cho cánh cửa sổ', 0.195),
('GK454', 'Máng che cánh cửa sổ', 0.288),

--('GK1215', 'Ke cửa sổ', 0.959), --duplicate

--MẶT CẮT PROFILE LAN CAN KÍNH
('E1214', 'Khung bao ngang trên', 1.795),
('E1215', 'Khung bao dưới', 0.976),
('E1216', 'Đố Lan Can', 1.131),
('B1735', 'Đố Lan Can', 1.347),

('E1217', 'Nối góc 90 độ', 1.453),
('E1218', 'Nắp đậy che rãnh', 0.110),

('B2831', 'Khung bao ngang trên', 1.402),
('B2832', 'Nắp đậy che rãnh', 0.177),
('B2846', 'Khung đứng', 1.081),
('B2833', 'Đố Lan Can', 1.404),

('B2834', 'Nối góc 90 độ', 1.617),
('B2835', 'Nắp đậy rãnh khung đứng', 0.109),

('B4425', 'Khung bao ngang trên', 1.453),
('B4426', 'Nẹp kính', 0.155),
('B4429', 'Khung bao đứng', 0.765),
('B4428', 'Đố lan can', 0.932),

('B4430', 'Nẹp kính', 0.153),
('B4427', 'Nối góc 90 độ', 1.197),

('B3730', 'Khung bao ngang trên', 1.128),
('B3731', 'Đố đứng', 0.920),
('B3732', 'Khung đứng', 0.689),
('B3733', 'Nẹp kính', 0.136);
