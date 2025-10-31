import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import taskRoutes from './routes/taskRoutes.js';

// โหลดไฟล์ .env เพื่อให้ผู้เข้าสอบสามารถตั้งค่าพอร์ตและการเชื่อมต่อฐานข้อมูลได้ง่าย
dotenv.config();

const app = express();

// กำหนดค่าเริ่มต้นของพอร์ตให้สามารถ override ผ่าน ENV ได้
const PORT = process.env.PORT ?? 4000;

// เปิดใช้งาน CORS เพื่อให้ Front-end เข้าถึง API ได้จากโดเมนอื่น
app.use(cors());

// รองรับ request body ที่เป็น JSON ซึ่งเป็นรูปแบบที่ใช้บ่อยที่สุด
app.use(express.json());

// รวมเส้นทางของโมดูลงาน (Task) และสามารถเพิ่มโมดูลอื่น ๆ ได้ในอนาคต
app.use('/api/tasks', taskRoutes);

// health check endpoint สำหรับตรวจสอบสถานะของเซิร์ฟเวอร์อย่างรวดเร็ว
app.get('/api/health', (_, res) => {
  res.json({ status: 'ok' });
});

// เริ่มต้นเซิร์ฟเวอร์และแสดงข้อความช่วย Debug
app.listen(PORT, () => {
  console.log(`Backend server is running on port ${PORT}`);
});
