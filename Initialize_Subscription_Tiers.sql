/*
    Aviah Collection ERP - Subscription Tiers Setup
    This script creates the table for managing the system's active subscription tier.
    Tiers: Basic, Standard, Premium
*/

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SubscriptionSettings')
BEGIN
    CREATE TABLE SubscriptionSettings (
        SettingID INT PRIMARY KEY IDENTITY(1,1),
        TierName NVARCHAR(50) NOT NULL, -- 'Basic', 'Standard', 'Premium'
        LastUpdated DATETIME DEFAULT GETDATE(),
        UpdatedBy NVARCHAR(100)
    );

    -- Insert default Basic Tier
    INSERT INTO SubscriptionSettings (TierName, UpdatedBy)
    VALUES ('Basic', 'System Initializer');
    
    PRINT 'SubscriptionSettings table created and initialized to Premium Tier.';
END
ELSE
BEGIN
    PRINT 'SubscriptionSettings table already exists.';
END
GO

-- Helper view to check current status
IF EXISTS (SELECT * FROM sys.views WHERE name = 'v_CurrentSubscription')
    DROP VIEW v_CurrentSubscription;
GO

CREATE VIEW v_CurrentSubscription AS
SELECT TOP 1 TierName, LastUpdated FROM SubscriptionSettings ORDER BY SettingID DESC;
GO

PRINT 'Subscription Tier setup complete.';
