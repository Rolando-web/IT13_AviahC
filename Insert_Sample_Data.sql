-- Insert sample data for Promotions
INSERT INTO Promotions (Name, Status, UsageCount, Progress, EndDate)
VALUES 
('Summer Sale 2024', 'Active', 850, 85, '2024-08-31'),
('New User Discount', 'Active', 1200, 60, '2024-12-31'),
('Flash Sale: Accessories', 'Inactive', 500, 100, '2024-05-01'),
('Loyalty Reward', 'Active', 300, 30, '2024-10-15');

-- Insert sample data for Deliveries (Logistics)
INSERT INTO Deliveries (DeliveryID, OrderRef, CustomerName, Status, ETA, DriverName, CurrentLocation, Destination)
VALUES 
('DEL-8401', 'ORD-984', 'Jayr Luayon', 'In Transit', '45 mins', 'Mike Ross', 'Downtown Metro', '123 Main St, NY'),
('DEL-8402', 'ORD-982', 'Sarah Johnson', 'Delivered', '-', 'Sarah Connor', 'Uptown', '456 Park Ave, NY'),
('DEL-8403', 'ORD-990', 'Mike Chen', 'Pending', 'TBD', 'Unassigned', 'Warehouse A', '789 Broadway, NY'),
('DEL-8404', 'ORD-995', 'Anna Belle', 'Pending', 'TBD', 'Unassigned', 'Warehouse B', '321 Pine St, CA');
