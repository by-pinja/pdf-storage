import * as express from 'express';

const app: express.Application = express();
const port: number = process.env.PORT || 3000;

app.listen(port, () => {
    // Success callback
    console.log(`Listening at http://localhost:${port}/`);
});