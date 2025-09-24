USE [PosHC]
GO
INSERT [poshc].[CatalogItem] ([Id], [Name], [UnitPrice], [Type], [Settings]) VALUES (N'f1aab99c-f9b5-4c29-a36b-02b7cafb5867', N'X-Ray', CAST(150.00 AS Decimal(10, 2)), 1, N'{"Name": "Chest X-Ray"}')
GO
INSERT [poshc].[CatalogItem] ([Id], [Name], [UnitPrice], [Type], [Settings]) VALUES (N'ecf5be69-0b9c-4115-8e27-c90da3dec9dc', N'Consultation', CAST(100.00 AS Decimal(10, 2)), 2, N'{"Duration": "30m", "Specialty": "Cardiology"}')
GO
INSERT [poshc].[CatalogItem] ([Id], [Name], [UnitPrice], [Type], [Settings]) VALUES (N'e0051fb4-f8f1-4659-b003-ccfadf6d3c68', N'Physiotherapy', CAST(200.00 AS Decimal(10, 2)), 2, N'{"Duration": "45m", "Specialty": "Orthopedics"}')
GO
INSERT [poshc].[CatalogItem] ([Id], [Name], [UnitPrice], [Type], [Settings]) VALUES (N'641988a9-ca99-452e-8251-fcae4bd5f8f5', N'Blood Test', CAST(50.00 AS Decimal(10, 2)), 1, N'{"Name": "Lab Test"}')
GO
INSERT [poshc].[Doctor] ([Id], [FirstName], [LastName], [Fee]) VALUES (N'98121082-a4f4-4e24-8ce3-2489ac31533c', N'Rim', N'Ahmad', CAST(50.00 AS Decimal(10, 2)))
GO
INSERT [poshc].[Doctor] ([Id], [FirstName], [LastName], [Fee]) VALUES (N'dd0d0441-0fba-4588-9015-783bd6b4389a', N'Ali', N'Hassan', CAST(75.00 AS Decimal(10, 2)))
GO
INSERT [poshc].[Doctor] ([Id], [FirstName], [LastName], [Fee]) VALUES (N'597f30cd-e5fd-4094-a887-846c5838eb14', N'Sara', N'Abdallah', CAST(150.00 AS Decimal(10, 2)))
GO
INSERT [poshc].[Doctor] ([Id], [FirstName], [LastName], [Fee]) VALUES (N'ef6c27d9-4fe4-47a4-b411-b29f807fc0ab', N'Mona', N'Khalil', CAST(120.00 AS Decimal(10, 2)))
GO
INSERT [poshc].[Doctor] ([Id], [FirstName], [LastName], [Fee]) VALUES (N'98ee52de-53c2-422d-8ad9-c6a7bb25c537', N'Karim', N'Saleh', CAST(95.50 AS Decimal(10, 2)))
GO
INSERT [poshc].[Doctor] ([Id], [FirstName], [LastName], [Fee]) VALUES (N'7614e865-bc5c-47b8-9acf-f16d52ad3206', N'George', N'Nasr', CAST(200.00 AS Decimal(10, 2)))
GO
INSERT [poshc].[Patient] ([Id], [FirstName], [LastName]) VALUES (N'dbbf08df-d121-44af-90c9-1e526ef4a610', N'Jane', N'Smith')
GO
INSERT [poshc].[Patient] ([Id], [FirstName], [LastName]) VALUES (N'f77ac0b8-f178-4494-bc52-5517e460b46a', N'Emily', N'Johnson')
GO
INSERT [poshc].[Patient] ([Id], [FirstName], [LastName]) VALUES (N'631ede0b-abcf-4504-ade8-856350b54008', N'John', N'Doe')
GO
INSERT [poshc].[Patient] ([Id], [FirstName], [LastName]) VALUES (N'2ad1d817-ae0a-4f87-a498-8a8c6a11697c', N'Michael', N'Brown')
GO
INSERT [poshc].[PaymentType] ([Id], [Name]) VALUES (2, N'Card')
GO
INSERT [poshc].[PaymentType] ([Id], [Name]) VALUES (1, N'Cash')
GO
INSERT [poshc].[PaymentType] ([Id], [Name]) VALUES (4, N'On-Account')
GO
INSERT [poshc].[PaymentType] ([Id], [Name]) VALUES (3, N'Transfer')
GO
