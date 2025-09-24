USE [PosHC]
GO
/****** Object:  Table [poshc].[CatalogItem]    Script Date: 9/25/2025 2:19:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [poshc].[CatalogItem](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[UnitPrice] [decimal](10, 2) NOT NULL,
	[Type] [int] NOT NULL,
	[Settings] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_CatalogItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [poshc].[Doctor]    Script Date: 9/25/2025 2:19:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [poshc].[Doctor](
	[Id] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Fee] [decimal](10, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [poshc].[Invoice]    Script Date: 9/25/2025 2:19:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [poshc].[Invoice](
	[Id] [uniqueidentifier] NOT NULL,
	[DoctorId] [uniqueidentifier] NOT NULL,
	[PatientId] [uniqueidentifier] NOT NULL,
	[Discount] [decimal](18, 2) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[DoctorFee] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [poshc].[InvoiceItem]    Script Date: 9/25/2025 2:19:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [poshc].[InvoiceItem](
	[Id] [uniqueidentifier] NOT NULL,
	[InvoiceId] [uniqueidentifier] NOT NULL,
	[CatalogItemId] [uniqueidentifier] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[LineTotal]  AS ([Quantity]*[UnitPrice]) PERSISTED,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [poshc].[Patient]    Script Date: 9/25/2025 2:19:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [poshc].[Patient](
	[Id] [uniqueidentifier] NOT NULL,
	[FirstName] [nvarchar](200) NOT NULL,
	[LastName] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_Patient] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [poshc].[Payment]    Script Date: 9/25/2025 2:19:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [poshc].[Payment](
	[Id] [uniqueidentifier] NOT NULL,
	[PaymentTypeId] [int] NOT NULL,
	[PaymentDate] [datetime2](7) NOT NULL,
	[Settings] [nvarchar](max) NULL,
	[InvoiceId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [poshc].[PaymentType]    Script Date: 9/25/2025 2:19:08 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [poshc].[PaymentType](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [poshc].[Doctor] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [poshc].[Doctor] ADD  DEFAULT ((0)) FOR [Fee]
GO
ALTER TABLE [poshc].[Invoice] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [poshc].[Invoice] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [poshc].[Invoice] ADD  DEFAULT ((0)) FOR [DoctorFee]
GO
ALTER TABLE [poshc].[InvoiceItem] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [poshc].[InvoiceItem] ADD  DEFAULT ((1)) FOR [Quantity]
GO
ALTER TABLE [poshc].[Payment] ADD  CONSTRAINT [DF__Payment__Payment__55009F39]  DEFAULT (sysutcdatetime()) FOR [PaymentDate]
GO
ALTER TABLE [poshc].[Invoice]  WITH CHECK ADD  CONSTRAINT [FK_Invoice_Doctor] FOREIGN KEY([DoctorId])
REFERENCES [poshc].[Doctor] ([Id])
GO
ALTER TABLE [poshc].[Invoice] CHECK CONSTRAINT [FK_Invoice_Doctor]
GO
ALTER TABLE [poshc].[Invoice]  WITH CHECK ADD  CONSTRAINT [FK_Invoice_Patient] FOREIGN KEY([PatientId])
REFERENCES [poshc].[Patient] ([Id])
GO
ALTER TABLE [poshc].[Invoice] CHECK CONSTRAINT [FK_Invoice_Patient]
GO
ALTER TABLE [poshc].[InvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceItem_CatalogItem] FOREIGN KEY([CatalogItemId])
REFERENCES [poshc].[CatalogItem] ([Id])
GO
ALTER TABLE [poshc].[InvoiceItem] CHECK CONSTRAINT [FK_InvoiceItem_CatalogItem]
GO
ALTER TABLE [poshc].[InvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK_InvoiceItem_Invoice] FOREIGN KEY([InvoiceId])
REFERENCES [poshc].[Invoice] ([Id])
GO
ALTER TABLE [poshc].[InvoiceItem] CHECK CONSTRAINT [FK_InvoiceItem_Invoice]
GO
ALTER TABLE [poshc].[Payment]  WITH CHECK ADD  CONSTRAINT [FK_Payment_Invoice] FOREIGN KEY([InvoiceId])
REFERENCES [poshc].[Invoice] ([Id])
GO
ALTER TABLE [poshc].[Payment] CHECK CONSTRAINT [FK_Payment_Invoice]
GO
ALTER TABLE [poshc].[Payment]  WITH CHECK ADD  CONSTRAINT [FK_Payment_PaymentType] FOREIGN KEY([PaymentTypeId])
REFERENCES [poshc].[PaymentType] ([Id])
GO
ALTER TABLE [poshc].[Payment] CHECK CONSTRAINT [FK_Payment_PaymentType]
GO
