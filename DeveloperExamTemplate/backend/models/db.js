import pkg from 'pg';
import dotenv from 'dotenv';

dotenv.config();

const { Pool } = pkg;

// กำหนดค่าการเชื่อมต่อฐานข้อมูลจากตัวแปรสภาพแวดล้อมเพื่อความยืดหยุ่น
export const pool = new Pool({
  host: process.env.DB_HOST ?? 'localhost',
  port: Number(process.env.DB_PORT ?? 5432),
  database: process.env.DB_NAME ?? 'developer_exam',
  user: process.env.DB_USER ?? 'postgres',
  password: process.env.DB_PASSWORD ?? 'postgres'
});

// ฟังก์ชันสำหรับทดสอบการเชื่อมต่อฐานข้อมูล
export const verifyConnection = async () => {
  try {
    await pool.query('SELECT 1');
    console.log('Database connection established successfully.');
  } catch (error) {
    console.error('Database connection failed:', error.message);
  }
};
