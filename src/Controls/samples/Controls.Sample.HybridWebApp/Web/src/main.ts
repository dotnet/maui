import './style.css'
import { message } from './message.ts'

const typescriptMessage = document.querySelector<HTMLParagraphElement>('#typescript-message')
const moduleMessage = document.querySelector<HTMLParagraphElement>('#module-message')

if (!typescriptMessage || !moduleMessage) {
  throw new Error('The demo content targets are missing from index.html.')
}

typescriptMessage.textContent = `TypeScript edit target: ${message}`

void import('./details.ts').then(({ details }) => {
  moduleMessage.textContent = details
})
