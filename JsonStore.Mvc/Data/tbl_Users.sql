/****** Object:  Table [dbo].[tbl_Users]    Script Date: 10/11/2016 10:17:52 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tbl_Users](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Email] [varchar](255) NOT NULL,
	[CreationTime] [datetime] NOT NULL CONSTRAINT [DF_tbl_Users_CreationTime]  DEFAULT (getdate()),
	[Active] [bit] NOT NULL,
	[RoleID] [bigint] NULL,
	[Password] [varchar](255) NULL,
	[IsReset] [bit] NOT NULL DEFAULT ((0)),
	[LastResetTime] [datetime] NULL,
	[LastActiveTime] [datetime] NULL,
	[PasswordHint] [varchar](1000) NULL,
	[ResetToken] [varchar](255) NULL,
 CONSTRAINT [PK_tbl_Users] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


