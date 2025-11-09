set client_encoding to 'utf8';

insert into roles ("name") values ('Director'), ('Technician'), ('Sale'), ('Inventory Manage'), ('Qa');

insert into users ("fullname", "phone", "password_hash", "role_id")
values ('Doãn Quốc Bảo', '0382633428', 'AQAAAAIAAYagAAAAEC7iGEcwGcYC51eb2ijKCRyIa18U40iGykiY27MJ06+6UzKwx/heauSLbMSeFifZag==', 1);

insert into materials ("code", "name", "weight", "type", "stock_length", "stock_width")
values
-- Cửa đi mở quay
('C3328', N'Khung cửa đi', 1),
('C3303', N'Cánh cửa đi mở ngoài (có gân)', 1),
('C18772', N'Cánh cửa đi mở ngoài (không gân)', 1),
('C3332', N'Cánh cửa đi mở trong (có gân)', 1),
('C18782', N'Cánh cửa đi mở trong (không gân)', 1),

('C3322', N'Cánh cửa đi mở ngoài (có gân & bo cạnh)', 1),
('C22912', N'Cánh cửa đi mở ngoài (không gân & bo cạnh)', 1),
('C38032', N'Cánh cửa đi ngang dưới (có gân)', 1),
('C3304', N'Cánh cửa đi ngang dưới (có gân)', 1),
('C6614', N'Cánh cửa đi ngang dưới (không gân)', 1),

('C3323', N'Đố động cửa đi', 1),
('C22903', N'Đố động cửa đi và cửa sổ', 1),
('C22900', N'Ốp đáy cánh cửa đi', 1),
('C3329', N'Ốp đáy cánh cửa đi', 1),
('C3319', N'Ngưỡng cửa đi', 1),

('C3291', N'Nẹp kính', 1),
('C3225', N'Nẹp kính', 1),
('C3296', N'Nẹp kính', 1),
('F347', N'Ke góc', 1),

('C3246', N'Nẹp kính', 1),
('C3286', N'Nẹp kính', 1),
('C3236', N'Nẹp kính', 1),
('C3206', N'Nẹp kính', 1),
('C3295', N'Nẹp kính', 1),

-- Cửa sổ mở quay
('C3318', N'Khung cửa sổ',  1),
('C8092', N'Cánh cửa sổ mở ngoài (có gân)', 1),
('C3202', N'Cánh cửa sổ mở ngoài (có gân)', 1),
('C18762', N'Cánh cửa sổ mở ngoài (không gân)', 1),
('C3312', N'Cánh cửa sổ mở ngoài (có gân & bo cạnh)', 1),

('C22922', N'Cánh cửa sổ mở ngoài (không gân & bo cạnh', 1),
('C3033', N'Đố động cửa sổ', 1),
--('C22903', N'Đố động cửa đi và cửa sổ', 1), --duplicate
('C3313', N'Đố cố định trên khung', 1),
('C3209', N'Khung vách kính', 1),

('C3203', N'Đố cố định (có lỗ vít)', 1),
('F077', N'Pano', 1),
('E1283', N'Khung lá sách', 1),
('E192', N'Lá sách', 1),
('B507', N'Nan dán trang trí', 1),

('C3300', N'Nối khung', 1),
('C3310', N'Nối khung', 1),
('C3210', N'Nối khung 90 độ (bo cạnh)', 1),
('C920', N'Nối khung 90 độ (vuông cạnh)', 1),
('C910', N'Nối khung 135 độ', 1),

('C459', N'Thanh truyền khóa', 1),

('C3317', N'Pát liên kết (đố cố định với Fix)', 1),
('C3207', N'Pát liên kết (đố cố định với Fix)', 1),
('C1687', N'Ke góc', 1),
('C4137', N'Ke góc', 1),
('C1697', N'Ke góc', 1),

--MẶT CẮT THANH NHÔM CỬA ĐI VÀ CỬA SỔ MỞ QUAY
('C38019', 'Khung cửa đi bản 100', 1),
('C38038', 'Khung cửa sổ bản 100', 1),
('C38039', 'Khung vách kính bản 100 (loại 1 nẹp)', 1),
('C48949', 'Khung vách kính bản 100 (loại 2 nẹp)', 1),

('C48954', 'Đố cố định bản 100 (loại 1 nẹp)', 1),
('C48953', 'Đố cố định bản 100 (loại 2 nẹp)', 1),
('C38010', 'Nối khung bản 100', 1),
('C48980', 'Nối khung 90 độ bản 100', 1),

('C48945', 'Nẹp phụ bản 100', 1),
--('F347', 'Ke góc', 1), --duplicate
--('C1687', 'Ke góc', 1), --duplicate
--('C4137', 'Ke góc', 1), --duplicate

--MẶT CẮT THANH NHÔM CỬA ĐI MỞ QUAY
('CX283', 'Khung cửa đi', 1),
('CX281', 'Cánh cửa đi mở ngoài', 1),
('CX282', 'Cánh ngang dưới cửa đi', 1),
('CX568', 'Đố động cửa đi', 1),
('CX309', 'Nối khung', 1),

--('C22900', 'Ốp đáy cánh cửa đi', 1), --duplicate
--('C3329', 'Ốp đáy cánh cửa đi', 1), --duplicate
--('C3319', 'Ngưỡng cửa đi', 1), --duplicate
--('C459', 'Thanh truyền khóa', 1), --duplicate
--('B507', 'Nan dán trang trí', 1), --duplicate

--('F347', 'Ke góc', 1), --duplicate

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ QUAY
('CX267', 'Khung cửa sổ, vách kính', 1),
('CX264', 'Cánh cửa sổ mở ngoài', 1),
('CX750', 'Đố động cửa sổ', 1),
('CX266', 'Đố cố định (có lỗ vít)', 1),
('CX265', 'Đố cố định (không lỗ vít)', 1),

('C25899', 'Khung bao chuyển hướng', 1),
('CX311', 'Nối khung vách kính', 1),
('CX310', 'Thanh nối góc 90 độ', 1),

--('C3246', 'Nẹp kính', 1), --duplicate
--('C3286', 'Nẹp kính', 1), --duplicate
--('C3236', 'Nẹp kính', 1), --duplicate
--('C3206', 'Nẹp kính', 1), --duplicate
--('C3295', 'Nẹp kính', 1), --duplicate

--('C1687', 'Ke góc', 1), --duplicate
--('C4137', 'Ke góc', 1), --duplicate
--('C1697', 'Ke góc', 1), --duplicate
('C1757', 'Ke góc', 1),

--MẶT CẮT THANH NHÔM CỬA ĐI VÀ CỬA SỔ MỞ QUAY
('C40988', 'Khung cửa đi và cửa sổ', 1),
('C48952', 'Cánh cửa đi vát cạnh bằng', 1),
('C40912', 'Cánh cửa đi vát cạnh lệch', 1),
('C48942', 'Cánh cửa sổ vát cạnh bằng', 1),
('C40902', 'Cánh cửa sổ vát cạnh lệch', 1),

('C40983', 'Đố cố định vát cạnh bằng', 1),
('C40984', 'Đố cố định vát cạnh lệch', 1),
('C44249', 'Khung vách kính', 1),
('C44234', 'Đố cố định (có lỗ vít)', 1),
('C40869', 'Đố động cửa đi và cửa sổ', 1),

('C40973', 'Đố cố định trên khung', 1),
('C40978', 'Ốp đáy cánh cửa đi', 1),
('E17523', 'Pano', 1),
('C44226', 'Nẹp kính', 1),
('C40979', 'Nẹp kính', 1),

--MẶT CẮT THANH NHÔM CỬA ĐI XẾP TRƯỢT
('F605', 'Khung ngang trên', 1),
('F606', 'Khung đứng', 1),
('F4116', 'Khung đứng (khoá đa điểm)', 1),
('F607', 'Khung ngang dưới (ray nổi)', 1),
('F2435', 'Khung ngang dưới (ray âm)', 1),

('F523', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1),
('F4117', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1),
('F5017', 'Cánh cửa không lỗ vít (khoá đa điểm)', 1),
('F522', 'Cánh cửa có lỗ vít (khoá đơn điểm)', 1),
('F560', 'Đố cố định trên cánh', 1),

('F520', 'Ốp giữa 2 cánh mở', 1),
('F519', 'Ốp che nước mưa', 1),
('F6029', 'Nẹp kính', 1),
('F521', 'Nẹp kính', 1),

('F608', 'Ke liên kết khung đứng với ngang trên', 1),
('F609', 'Ke liên kết khung đứng với ngang dưới', 1),
('F417', 'Ke góc', 1),
--('F347', 'Ke góc', 1), --duplicate

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ LÙA
('D23151', 'Khung cửa lùa', 1),
('D45482', 'Khung cửa lùa 3 ray', 1),
('D23156', 'Cánh cửa lùa', 1),
('D23157', 'Ốp cánh cửa lùa', 1),
('D23158', 'Nẹp đối đầu cửa 4 cánh', 1),

('D23159', 'Ốp che nước mưa', 1),

--MẶT CẮT THANH NHÔM CỬA SỔ MỞ LÙA
('D44329', 'Khung cửa lùa', 1),
('D44035', 'Cánh cửa mở lùa', 1),
('D44327', 'Ốp cánh cửa lùa', 1),
('D44328', 'Nẹp đối đầu cửa 4 cánh', 1),

--MẶT CẮT THANH NHÔM CỬA ĐI MỞ LÙA
('D47713', 'Khung cửa lùa', 1),
('D45316', 'Cánh cửa mở lùa', 1),
('D44564', 'Cánh cửa mở lùa', 1),
('D47688', 'Nẹp đối đầu cửa 4 cánh', 1),
('D46070', 'Ốp khóa đa điểm', 1),

('D47679', 'Ốp đậy ray', 1),
('D47678', 'Ốp đậy rãnh phụ kiện', 1),
('D45478', 'Thanh ốp móc', 1),
('D44569', 'Nẹp kính', 1),

--MẶT CẮT THANH NHÔM CỬA MỞ LÙA
('D1541A', 'Khung ngang trên', 1),
('D1551A', 'Đố chia cửa lùa với vách kính trên', 1),
('D17182', 'Khung ngang dưới (ray bằng)', 1),
('D1942', 'Khung ngang dưới (ray lệch)', 1),
('D1542A', 'Khung ngang dưới (ray lệch)', 1),

('D1543A', 'Khung đứng', 1),
('D3213', 'Khung đứng (3 ray)', 1),
('D3211', 'Khung ngang trên (3 ray)', 1),
('D3212', 'Khung ngang dưới (3 ray)', 1),
('D1544A', 'Cánh ngang trên', 1),

('D1545A', 'Cánh ngang dưới', 1),
('D1546A', 'Cánh đứng trơn', 1),
('D1547A', 'Cánh đứng móc', 1),
('D28144', 'Cánh ngang trên', 1),
('D1555A', 'Cánh ngang dưới', 1),

('D26146', 'Cánh đứng trơn', 1),
('D28127', 'Cánh đứng móc', 1),
('D1559A', 'Khung đứng vách kính', 1),
('D2618', 'Đố cố định trên vách kính', 1),
('D1354', 'Đố cố định trên cánh', 1),

('D1548A', 'Nẹp đối đầu cửa 4 cánh', 1),
('D1549A', 'Ốp khung vách kính', 1),
('D1578', 'Nối khung vách kính', 1),
('D2420', 'Nối góc 90 độ trái', 1),
('D2490', 'Nối góc 90 độ phải', 1),

('D34608', 'Thanh chuyển kính hộp', 1),
('D1779', 'Nẹp kính', 1),
('D1298', 'Nẹp kính', 1),
('D1168', 'Nẹp kính', 1),
('C101', 'Nẹp kính', 1),

--MẶT CẮT THANH NHÔM CỬA BẢN LỀ SÀN
('F631', 'Cánh đứng', 1),
('F632', 'Cánh ngang trên', 1),
('F633', 'Cánh ngang dưới', 1),

('F2084', 'Đố tĩnh', 1),
('F630', 'Nẹp kính', 1),
('F949', 'Nẹp kính', 1),

--MỘT SỐ MÃ PHỤ
('D47680', 'Ngưỡng nhôm', 1),
('A1079', 'Nẹp lưới chống muỗi', 1),
('A1080', 'Nẹp lưới chống muỗi', 1),
('D47590', 'Ray nhôm cho cửa nhựa', 1),

--MẶT CẮT THANH NHÔM MẶT DỰNG LỘ ĐỐ
('GK461', 'Thanh đố đứng', 1),
('GK471', 'Thanh đố đứng', 1),
('GK481', 'Thanh đố đứng', 1),
('GK491', 'Thanh đố đứng', 1),

('GK501', 'Thanh đố đứng', 1),
('E21451', 'Thanh đố đứng', 1),
('GK581', 'Thanh đố đứng (kính hộp)', 1),
('GK993', 'Thanh đố ngang', 1),

('GK2053', 'Thanh đố ngang', 1),
('GK2467', 'Thanh nêm đố ngang', 1),
('GK858', 'Pat liên kết thang ngang', 1),
('GK1073', 'Nắp đậy đố ngang', 1),

('GK015', 'Đế ốp mặt ngoài', 1),
('GK066', 'Nắp đậy đế ốp', 1),
('GK780', 'Nối góc 90 độ ngoài', 1),
('GK1495', 'Đế ốp mặt ngoài góc 90 độ', 1),

('GK806', 'Nắp đậy đế ốp góc 90 độ', 1),
('GK1035', 'Đế ốp mặt ngoài góc 135 độ', 1),
('GK606', 'Nắp đậy đế ốp góc 135 độ', 1),
('GK294', 'Nắp đậy che rãnh', 1),

('GK2464', 'Nắp đậy khe rãnh', 1),
('GK1255', 'Khung trên cửa sổ (dạng móc treo)', 1),
('GK1325', 'Cánh trên cửa sổ (dạng móc treo)', 1),
('GK1295', 'Khung cửa sổ', 1),

('GK1365', 'Cánh cửa sổ', 1),
('GK505', 'Thanh đố kính cho cánh cửa sổ', 1),
('GK1215', 'Ke cửa sổ', 1),

--THANH NHÔM MẶT DỰNG GIẤU ĐỐ
('GK001', 'Thanh đố đứng', 1),
('GK011', 'Thanh đố đứng', 1),
('GK021', 'Thanh đố đứng', 1),
('GK251', 'Thanh đố đứng', 1),

('GK261', 'Thanh đố đứng', 1),
('GK813', 'Thanh đố ngang', 1),
('GK853', 'Thanh đố ngang', 1),
('GK413', 'Nắp đậy thanh đố ngang', 1),

('GK1745', 'Pat liên kết thanh đố ngang', 1),
--('GK2467', 'Thanh nêm đố ngang', 1), --duplicate
('GK228', 'Nẹp kính trái', 1),
('GK238', 'Nẹp kính phải', 1),

('GK218', 'Nẹp kính trên', 1),
('GK208', 'Nẹp kính dưới', 1),
('GK255', 'Thanh móc treo kính', 1),
--('C459', 'Thanh truyền khóa', 1), --duplicate

('GK275', 'Thanh đố kính', 1),
('GK1064', 'Chống nhấc cánh', 1),
--('GK1255', 'Khung trên cửa sổ (dạng móc treo)', 1), --duplicate
--('GK1325', 'Cánh trên cửa sổ (dạng móc treo)', 1), --duplicate

--('GK1295', 'Khung cửa sổ', 1), --duplicate
--('GK1365', 'Cánh cửa sổ', 1), --duplicate
('GK534', 'Thanh đỡ kính cho cánh cửa sổ', 1),
('GK454', 'Máng che cánh cửa sổ', 1),

--('GK1215', 'Ke cửa sổ', 1), --duplicate

--MẶT CẮT PROFILE LAN CAN KÍNH
('E1214', 'Khung bao ngang trên', 1),
('E1215', 'Khung bao dưới', 1),
('E1216', 'Đố Lan Can', 1),
('B1735', 'Đố Lan Can', 1),

('E1217', 'Nối góc 90 độ', 1),
('E1218', 'Nắp đậy che rãnh', 1),

('B2831', 'Khung bao ngang trên', 1),
('B2832', 'Nắp đậy che rãnh', 1),
('B2846', 'Khung đứng', 1),
('B2833', 'Đố Lan Can', 1),

('B2834', 'Nối góc 90 độ', 1),
('B2835', 'Nắp đậy rãnh khung đứng', 1),

('B4425', 'Khung bao ngang trên', 1),
('B4426', 'Nẹp kính', 1),
('B4429', 'Khung bao đứng', 1),
('B4428', 'Đố lan can', 1),

('B4430', 'Nẹp kính', 1),
('B4427', 'Nối góc 90 độ', 1),

('B3730', 'Khung bao ngang trên', 1),
('B3731', 'Đố đứng', 1),
('B3732', 'Khung đứng', 1),
('B3733', 'Nẹp kính', 1);

INSERT INTO clients ("name", "address", "phone", "email", "sales_in_charge_id")
VALUES
('Albert Cook', '123 Đường ABC, Hà Nội', '090-123-4567', 'albert.cook@example.com', NULL),
('Barry Hunter', '456 Đường XYZ, TP. HCM', '091-234-5678', 'barry.hunter@example.com', NULL),
('Trevor Baker', '789 Đường QWE, Đà Nẵng', '092-345-6789', 'trevor.baker@example.com', NULL),
('Nguyễn Văn An', '101 Đường Hùng Vương, Huế', '098-888-9999', 'an.nguyen@company.vn', NULL),
('Trần Thị Bích', '22 Phố Cổ, Hà Nội', '097-777-6666', 'bich.tran@startup.com', NULL);

INSERT INTO projects ("name", "location", "client_id", "design_company", "completion_date", "created_at", "design_file_path", "status", "document_path")
VALUES
('Dự án Vinhome', 'Hà Nội', 1, 'Design Firm X', '2025-12-31', '2025-10-01 09:00:00', 'path/A.pdf', 'Active', 'doc/A.docx'),
('Dự án Ecopark', 'Hưng Yên', 2, 'Design Firm Y', '2024-10-20', '2025-10-05 10:00:00', 'path/B.pdf', 'Completed', 'doc/B.docx'),
('Dự án Biệt thự FLC', 'Quy Nhơn', 1, 'Design Firm X', '2026-06-15', '2025-10-10 11:00:00', 'path/C.pdf', 'Pending', 'doc/C.docx'),
('Khách sạn Imperial Huế', 'Huế', 4, 'Kiến trúc Sông Hương', '2026-03-01', '2025-10-15 14:00:00', 'designs/hue_imperial.pdf', 'Scheduled', 'rfq/imperial_docs.docx'),
('Homestay Phố Cổ', 'Hà Nội', 5, NULL, '2025-11-30', '2025-10-20 16:30:00', NULL, 'Pending', 'rfq/homestay_hanoi.docx');

INSERT INTO machine_types ("name") 
VALUES 
('Máy Cắt'),
('Máy Phay Ổ Khóa'),
('Máy Tiện Tự Động');

INSERT INTO machines 
("name", "machine_type_id", "status", "entry_date", "last_maintenance_date", "capacity_value", "capacity_unit", "expected_completion_date")
VALUES
('Máy Cắt CNC 01', 1, 'Operational', '2025-01-15', NULL, 'mm/phút', NULL),
('Máy Cắt Góc', 1, 'Operational', '2025-02-20', NULL, 5, 'mm', NULL),
('Máy Phay Ổ Khóa', 2, 'Operational', '2025-03-10', NULL, 50, 'sản phẩm/giờ', NULL);
