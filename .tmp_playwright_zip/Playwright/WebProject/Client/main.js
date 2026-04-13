import { Grad } from "./grad.js";

const container = document.createElement('div');
container.className = 'container';

document.body.appendChild(container);

const grad = new Grad(container);
grad.crtaj();