// MongoDB initialization script for QuiosqueFood3000 Kitchen
// This script will run when the MongoDB container starts for the first time

// Switch to the application database
db = db.getSiblingDB('QuiosqueFood3000Kitchen');

// Create collections that might be used by the Kitchen API
db.createCollection('orders');
db.createCollection('kitchenItems');
db.createCollection('preparationQueue');

// Create indexes for better performance
db.orders.createIndex({ "orderId": 1 }, { unique: true });
db.orders.createIndex({ "status": 1 });
db.orders.createIndex({ "createdAt": 1 });

db.kitchenItems.createIndex({ "itemId": 1 }, { unique: true });
db.kitchenItems.createIndex({ "category": 1 });

db.preparationQueue.createIndex({ "orderId": 1 });
db.preparationQueue.createIndex({ "priority": 1 });
db.preparationQueue.createIndex({ "status": 1 });

// Insert some sample data (optional - remove if not needed)
db.kitchenItems.insertMany([
    {
        itemId: "burger-001",
        name: "Classic Burger",
        category: "main",
        preparationTime: 8,
        ingredients: ["beef patty", "bun", "lettuce", "tomato", "cheese"],
        createdAt: new Date()
    },
    {
        itemId: "fries-001", 
        name: "French Fries",
        category: "side",
        preparationTime: 5,
        ingredients: ["potatoes", "salt"],
        createdAt: new Date()
    },
    {
        itemId: "drink-001",
        name: "Soft Drink",
        category: "beverage",
        preparationTime: 1,
        ingredients: ["soda"],
        createdAt: new Date()
    }
]);

print('Database initialized successfully for QuiosqueFood3000 Kitchen!');
