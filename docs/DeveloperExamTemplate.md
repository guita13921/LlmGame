# Developer Exam Template

เอกสารฉบับนี้อธิบายโครงสร้าง Template สำหรับการสอบ Developer ที่ครอบคลุมทั้งฝั่ง Front-end, Back-end, Database และ API เพื่อให้สามารถนำไปประยุกต์ใช้ได้อย่างรวดเร็ว

## โครงสร้างโดยรวม

```
DeveloperExamTemplate/
├── frontend/
├── backend/
├── database/
└── api/
```

แต่ละโฟลเดอร์แยกหน้าที่ชัดเจนเพื่อให้ผู้เข้าสอบสามารถโฟกัสกับภารกิจของตนเองได้

---

## Front-end

* **เทคโนโลยีหลัก:** React + Vite
* **ไฟล์สำคัญ:**
  * `frontend/index.html` – จุดเริ่มต้นของแอปพลิเคชัน
  * `frontend/src/main.jsx` – กำหนดการเรนเดอร์คอมโพเนนต์หลักเข้าสู่ DOM
  * `frontend/src/App.jsx` – ตัวอย่างการดึงข้อมูลจาก API, จัดการ state, และแสดงผล
  * `frontend/src/components/TaskForm.jsx` และ `frontend/src/components/TaskList.jsx` – แยกความรับผิดชอบของฟอร์มและรายการงาน
* **แนวทางการต่อยอด:**
  * เพิ่มระบบ Routing ด้วย React Router
  * เชื่อมต่อ State Management ภายนอก เช่น Redux หรือ Zustand
  * เพิ่มชุดทดสอบด้วย Vitest และ React Testing Library

---

## Back-end

* **เทคโนโลยีหลัก:** Node.js + Express
* **ไฟล์สำคัญ:**
  * `backend/server.js` – ตั้งค่าเซิร์ฟเวอร์ Express และเชื่อมต่อ Route ต่าง ๆ
  * `backend/routes/taskRoutes.js` – กำหนด Endpoints สำหรับงาน (Task)
  * `backend/controllers/taskController.js` – จัดการลอจิกของการรับ/ส่งข้อมูล
  * `backend/models/taskModel.js` และ `backend/models/db.js` – ติอต่อฐานข้อมูล PostgreSQL ผ่าน pg Pool
* **แนวทางการต่อยอด:**
  * เพิ่ม Middleware สำหรับ Authentication/Authorization
  * เขียน Unit Test ด้วย Vitest และ Integration Test ด้วย Supertest
  * แยก Service Layer เพื่อรองรับลอจิกที่ซับซ้อนขึ้น

---

## Database

* **เทคโนโลยีหลัก:** PostgreSQL
* **ไฟล์สำคัญ:**
  * `database/schema.sql` – สคริปต์สำหรับสร้างตาราง `tasks`
  * `database/seed.sql` – ข้อมูลตัวอย่างสำหรับเริ่มต้นระบบอย่างรวดเร็ว
* **แนวทางการต่อยอด:**
  * เพิ่มตารางอื่น ๆ เช่น `users`, `submissions`
  * ผูก Constraint และ Index เพิ่มเติมเพื่อรองรับปริมาณข้อมูลมากขึ้น
  * ตั้งค่าเครื่องมือ Migration เช่น Prisma หรือ Knex

---

## API

* **ไฟล์สำคัญ:**
  * `api/tasks.http` – ตัวอย่างคำสั่ง HTTP ที่ใช้ทดสอบ API ด้วย REST Client
* **Endpoints ที่มีให้:**
  * `GET /api/health` – ตรวจสอบสถานะเซิร์ฟเวอร์
  * `GET /api/tasks` – ดึงรายการงานทั้งหมด
  * `POST /api/tasks` – สร้างงานใหม่โดยรับค่า `title` และ `description`
* **แนวทางการต่อยอด:**
  * เพิ่มการแก้ไข (`PUT /api/tasks/:id`) และลบงาน (`DELETE /api/tasks/:id`)
  * รองรับ Query Parameters สำหรับการค้นหาและกรองข้อมูล
  * ออกแบบเอกสาร API ด้วย OpenAPI/Swagger

---

## การเริ่มต้นใช้งานอย่างรวดเร็ว

1. สร้างฐานข้อมูลตามไฟล์ `database/schema.sql` และเติมข้อมูลจาก `database/seed.sql`
2. คัดลอกไฟล์ `backend/.env.example` เป็น `.env` และแก้ไขค่าให้ตรงกับเครื่องของคุณ
3. ติดตั้ง Dependencies ของแต่ละส่วนด้วย `npm install` ภายในโฟลเดอร์ `frontend` และ `backend`
4. รัน Back-end ด้วย `npm run dev` ในโฟลเดอร์ `backend`
5. รัน Front-end ด้วย `npm run dev` ในโฟลเดอร์ `frontend` และเปิดเบราว์เซอร์ที่ http://localhost:5173

---

## เคล็ดลับสำหรับผู้ออกข้อสอบ

* กำหนดโจทย์เพิ่มเติม เช่น การจัดการสถานะงาน (Done/In Progress), ระบบผู้ใช้, หรือการแสดงผลกราฟ
* เพิ่ม Test Case อัตโนมัติเพื่อตรวจสอบผลงานของผู้เข้าสอบได้อย่างรวดเร็ว
* ปรับแต่งเอกสารนี้ให้สอดคล้องกับเกณฑ์การประเมินขององค์กร

---

ขอให้สนุกกับการสร้างแบบทดสอบ และหวังว่า Template นี้จะช่วยให้การจัดสอบเป็นเรื่องง่ายขึ้น!
