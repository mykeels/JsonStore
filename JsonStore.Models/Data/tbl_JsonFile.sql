/****** Object:  Table [dbo].[tbl_JsonFile]    Script Date: 10/11/2016 10:16:43 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tbl_JsonFile](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[Description] [varchar](max) NULL,
	[Path] [varchar](max) NULL,
	[CreationTime] [datetime] NULL,
	[LastUpdateTime] [datetime] NULL,
	[Active] [bit] NULL,
	[BackupOf] [bigint] NULL,
	[UserRoleID] [bigint] NOT NULL DEFAULT ((7)),
 CONSTRAINT [PK_JsonFiles] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


