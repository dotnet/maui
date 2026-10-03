const conceptContent = {
  tokens: {
    kicker: 'THE INPUT LAYER',
    title: 'Tokens are not always words.',
    body: 'A tokenizer converts text into integer IDs from a fixed vocabulary. Common words may be one token; unusual words may split into several. Tokenization affects context limits, cost, speed, and how easily a model handles different languages.',
    analogy: 'movable type in a printing press.',
    gradient: 'linear-gradient(90deg, #ff6534 0 17%, transparent 17% 21%, #4f6fff 21% 50%, transparent 50% 54%, #11130f 54% 78%, transparent 78% 82%, #d7ff3f 82%)'
  },
  embeddings: {
    kicker: 'THE REPRESENTATION LAYER',
    title: 'Meaning becomes geometry.',
    body: 'Each token ID maps to a learned vector: a long list of numbers. During processing, those representations become contextual—so “bank” near “river” differs from “bank” near “loan.” Similar patterns occupy related regions of a high-dimensional space.',
    analogy: 'placing ideas on a map where distance carries meaning.',
    gradient: 'radial-gradient(circle at 18% 30%, #4f6fff 0 8%, transparent 9%), radial-gradient(circle at 60% 72%, #ff6534 0 7%, transparent 8%), radial-gradient(circle at 83% 25%, #11130f 0 6%, transparent 7%), linear-gradient(135deg, transparent 0 42%, #d7ff3f 43% 48%, transparent 49%)'
  },
  attention: {
    kicker: 'THE ROUTING LAYER',
    title: 'Every token asks what matters.',
    body: 'Self-attention computes how strongly each position should draw information from other positions. Multiple attention heads can track different relationships, such as syntax, reference, or local patterns. It is dynamic information routing, not human attention.',
    analogy: 'a meeting where every word chooses whom to listen to.',
    gradient: 'repeating-conic-gradient(from 20deg at 50% 50%, #11130f 0deg 7deg, transparent 8deg 28deg), radial-gradient(circle, #ff6534 0 9%, transparent 10% 100%)'
  },
  prediction: {
    kicker: 'THE OUTPUT LAYER',
    title: 'Generation is a repeated wager.',
    body: 'The network produces a score for every vocabulary token. Those scores become probabilities, a decoding strategy selects one token, and the cycle repeats with the new token added to the context. A fluent answer is therefore a chain of local predictions.',
    analogy: 'autocomplete looping faster and with a much richer model of context.',
    gradient: 'linear-gradient(135deg, #11130f 0 18%, transparent 18% 25%, #4f6fff 25% 43%, transparent 43% 50%, #ff6534 50% 68%, transparent 68% 75%, #d7ff3f 75%)'
  }
};

const conceptTabs = [...document.querySelectorAll('.concept-tab')];
const conceptPanel = document.getElementById('conceptPanel');

function selectConcept(tab) {
  const concept = conceptContent[tab.dataset.concept];
  conceptTabs.forEach((item) => {
    const selected = item === tab;
    item.classList.toggle('active', selected);
    item.setAttribute('aria-selected', String(selected));
  });
  document.getElementById('conceptKicker').textContent = concept.kicker;
  document.getElementById('conceptTitle').textContent = concept.title;
  document.getElementById('conceptBody').textContent = concept.body;
  document.getElementById('conceptAnalogy').innerHTML = `<strong>Think of it like:</strong> ${concept.analogy}`;
  document.getElementById('conceptDiagram').style.background = concept.gradient;
}

conceptTabs.forEach((tab, index) => {
  tab.addEventListener('click', () => selectConcept(tab));
  tab.addEventListener('keydown', (event) => {
    if (!['ArrowDown', 'ArrowUp', 'ArrowRight', 'ArrowLeft'].includes(event.key)) return;
    event.preventDefault();
    const direction = ['ArrowDown', 'ArrowRight'].includes(event.key) ? 1 : -1;
    const next = conceptTabs[(index + direction + conceptTabs.length) % conceptTabs.length];
    next.focus();
    selectConcept(next);
  });
});

const tokenInput = document.getElementById('tokenInput');
const tokenOutput = document.getElementById('tokenOutput');
const tokenCount = document.getElementById('tokenCount');
const tokenColors = ['#d7ff3f', '#ff6534', '#8aa0ff', '#f0ba60', '#8ce3b2'];

function simulatedTokenize(text) {
  const pieces = text.match(/[A-Za-z]+(?:'[A-Za-z]+)?|\d+|[^\s\w]/g) || [];
  return pieces.flatMap((piece) => {
    if (!/^[A-Za-z]/.test(piece) || piece.length <= 7) return [piece];
    const splitAt = Math.max(3, Math.min(6, Math.ceil(piece.length * 0.55)));
    return [piece.slice(0, splitAt), `##${piece.slice(splitAt)}`];
  });
}

function renderTokens() {
  const tokens = simulatedTokenize(tokenInput.value);
  tokenOutput.replaceChildren();
  tokens.forEach((token, index) => {
    const chip = document.createElement('span');
    chip.className = 'token-chip';
    chip.textContent = token;
    chip.style.setProperty('--chip-color', tokenColors[index % tokenColors.length]);
    chip.style.animationDelay = `${index * 25}ms`;
    tokenOutput.appendChild(chip);
  });
  tokenCount.textContent = `${tokens.length} token${tokens.length === 1 ? '' : 's'}`;
}

document.getElementById('tokenizeButton').addEventListener('click', renderTokens);
tokenInput.addEventListener('input', renderTokens);
renderTokens();

const baseCandidates = [
  { label: 'a garden', logit: 3.2 },
  { label: 'nothing', logit: 2.3 },
  { label: 'the ocean', logit: 1.8 },
  { label: 'itself', logit: 1.1 },
  { label: 'Tuesday', logit: 0.1 }
];
const temperatureRange = document.getElementById('temperatureRange');
const temperatureValue = document.getElementById('temperatureValue');
const sampleBars = document.getElementById('sampleBars');
const sampleResult = document.getElementById('sampleResult');
let currentProbabilities = [];

function softmaxAtTemperature(values, temperature) {
  const scaled = values.map((value) => value / temperature);
  const max = Math.max(...scaled);
  const exponents = scaled.map((value) => Math.exp(value - max));
  const total = exponents.reduce((sum, value) => sum + value, 0);
  return exponents.map((value) => value / total);
}

function renderTemperature() {
  const temperature = Number(temperatureRange.value);
  temperatureValue.textContent = temperature.toFixed(1);
  currentProbabilities = softmaxAtTemperature(baseCandidates.map((item) => item.logit), temperature);
  sampleBars.replaceChildren();

  baseCandidates.forEach((candidate, index) => {
    const percent = currentProbabilities[index] * 100;
    const row = document.createElement('div');
    row.className = 'sample-bar';
    row.innerHTML = `<code>${candidate.label}</code><span class="sample-bar-track"><i style="--bar-width:${percent}%"></i></span><b>${percent.toFixed(1)}%</b>`;
    sampleBars.appendChild(row);
  });
}

function sampleCandidate() {
  let draw = Math.random();
  let selectedIndex = currentProbabilities.length - 1;
  for (let index = 0; index < currentProbabilities.length; index += 1) {
    draw -= currentProbabilities[index];
    if (draw <= 0) {
      selectedIndex = index;
      break;
    }
  }
  sampleResult.textContent = baseCandidates[selectedIndex].label;
}

temperatureRange.addEventListener('input', renderTemperature);
document.getElementById('sampleButton').addEventListener('click', sampleCandidate);
renderTemperature();

const ragExamples = {
  context: {
    query: 'What limits how much an LLM can read?',
    passage: 'A context window is the maximum number of tokens a model can process in one request, including the prompt, retrieved text, tool results, and generated output.',
    score: 'SIMILARITY 0.94 · document: model-basics.md',
    answer: 'The context window limits how much the model can read at once. Everything sent and generated must fit within that token budget.'
  },
  rag: {
    query: 'Does RAG retrain the model?',
    passage: 'Retrieval-augmented generation fetches relevant documents at inference time and inserts them into the model context. It does not update the model parameters.',
    score: 'SIMILARITY 0.97 · document: rag-pattern.md',
    answer: 'No. RAG supplies retrieved evidence during a request; it does not retrain or change the model’s weights.'
  },
  hallucination: {
    query: 'Why do models invent facts?',
    passage: 'Language models optimize token prediction. When evidence is missing or patterns conflict, a plausible continuation can be fluent but unsupported.',
    score: 'SIMILARITY 0.91 · document: reliability.md',
    answer: 'Because the model generates likely text rather than verifying truth. Without strong evidence, plausibility can win over factual support.'
  }
};

function renderRag(key) {
  const example = ragExamples[key];
  document.getElementById('ragQuery').textContent = example.query;
  document.getElementById('ragPassage').textContent = example.passage;
  document.getElementById('ragScore').textContent = example.score;
  document.getElementById('ragAnswer').textContent = example.answer;
  document.querySelectorAll('.rag-question').forEach((button) => {
    button.classList.toggle('active', button.dataset.rag === key);
  });
}

document.querySelectorAll('.rag-question').forEach((button) => {
  button.addEventListener('click', () => renderRag(button.dataset.rag));
});
renderRag('context');

const promptElements = ['goalSelect', 'audienceSelect', 'formatSelect', 'topicInput', 'evidenceToggle'];
const goalText = {
  explain: 'Explain the topic clearly',
  summarize: 'Summarize the supplied material',
  compare: 'Compare the relevant options',
  critique: 'Critique the proposal constructively'
};
const audienceText = {
  beginner: 'a curious beginner with no assumed technical background',
  developer: 'a software developer who understands APIs and data structures',
  leader: 'a technical leader making an implementation decision',
  expert: 'a domain expert who values precision and nuance'
};
const formatText = {
  lesson: 'a short lesson with a plain-language definition, one concrete example, and a brief takeaway',
  steps: 'a numbered step-by-step guide with prerequisites and common failure points',
  table: 'a compact comparison table followed by a recommendation and its trade-offs',
  json: 'valid JSON with keys for summary, key_points, assumptions, risks, and next_steps'
};

function buildPrompt() {
  const goal = document.getElementById('goalSelect').value;
  const audience = document.getElementById('audienceSelect').value;
  const format = document.getElementById('formatSelect').value;
  const topic = document.getElementById('topicInput').value.trim() || '[Insert topic or material]';
  const needsEvidence = document.getElementById('evidenceToggle').checked;
  const evidenceInstruction = needsEvidence
    ? '\n- Separate claims supported by the provided material from general knowledge.\n- Cite the supplied evidence inline when possible.\n- State uncertainty and identify any claim that should be independently verified.'
    : '';

  document.getElementById('generatedPrompt').textContent =
`ROLE
You are a careful AI educator writing for ${audienceText[audience]}.

TASK
${goalText[goal]}:
${topic}

OUTPUT
Return ${formatText[format]}.

REQUIREMENTS
- Define specialized terms before using them.
- Prefer concrete examples over analogies alone.
- Do not invent sources, measurements, or quotations.${evidenceInstruction}

Before answering, silently check that the response follows every requirement.`;
}

promptElements.forEach((id) => {
  const element = document.getElementById(id);
  element.addEventListener('input', buildPrompt);
  element.addEventListener('change', buildPrompt);
});

document.getElementById('copyPrompt').addEventListener('click', async () => {
  const status = document.getElementById('copyStatus');
  try {
    await navigator.clipboard.writeText(document.getElementById('generatedPrompt').textContent);
    status.textContent = 'Copied to clipboard.';
  } catch {
    status.textContent = 'Clipboard unavailable. Select the prompt and copy it manually.';
  }
  window.setTimeout(() => {
    status.textContent = '';
  }, 2200);
});
buildPrompt();

const glossary = [
  ['Attention', 'A mechanism that lets each token combine information from other token positions using learned relevance scores.'],
  ['Benchmark', 'A standardized set of tasks and metrics used to compare models, often imperfectly and sometimes contaminated by training data.'],
  ['Chunking', 'Splitting documents into retrievable passages. Chunk size and overlap strongly affect RAG quality.'],
  ['Context window', 'The maximum token budget a model can process in one request, including input and generated output.'],
  ['DPO', 'Direct Preference Optimization: training that directly favors preferred responses over rejected ones.'],
  ['Embedding', 'A learned numeric vector representing a token, passage, image, or other item for comparison and processing.'],
  ['Fine-tuning', 'Updating some or all model weights with task-specific examples after pretraining.'],
  ['Hallucination', 'A fluent output that is unsupported, incorrect, or fabricated.'],
  ['Inference', 'Running a trained model to produce outputs rather than updating its weights.'],
  ['LoRA', 'Low-Rank Adaptation: a parameter-efficient fine-tuning method that trains small added matrices.'],
  ['Parameter', 'A learned numeric value in a neural network. Model size is often described by its parameter count.'],
  ['Quantization', 'Representing model weights with fewer bits to reduce memory and often speed up local inference.'],
  ['RAG', 'Retrieval-Augmented Generation: retrieving external evidence and adding it to the model context before generation.'],
  ['RLHF', 'Reinforcement Learning from Human Feedback: using preference data and a reward signal to shape model behavior.'],
  ['Temperature', 'A decoding control that sharpens or flattens next-token probabilities. It changes variability, not knowledge.'],
  ['Token', 'A vocabulary unit processed by a model: often a word fragment, punctuation mark, or special symbol.'],
  ['Transformer', 'A neural architecture built around attention and feed-forward blocks, introduced in 2017.'],
  ['Vector database', 'A system optimized for storing embeddings and retrieving nearby vectors by similarity.']
];

function renderGlossary(query = '') {
  const normalized = query.trim().toLowerCase();
  const matches = glossary.filter(([term, definition]) =>
    `${term} ${definition}`.toLowerCase().includes(normalized)
  );
  const grid = document.getElementById('glossaryGrid');
  grid.replaceChildren();

  if (matches.length === 0) {
    const empty = document.createElement('p');
    empty.className = 'glossary-empty';
    empty.textContent = 'No matching term. Try a broader word.';
    grid.appendChild(empty);
    return;
  }

  matches.forEach(([term, definition]) => {
    const item = document.createElement('article');
    item.className = 'glossary-item';
    const heading = document.createElement('h3');
    heading.textContent = term;
    const body = document.createElement('p');
    body.textContent = definition;
    item.append(heading, body);
    grid.appendChild(item);
  });
}

document.getElementById('glossarySearch').addEventListener('input', (event) => {
  renderGlossary(event.target.value);
});
renderGlossary();

const sourceFilters = [...document.querySelectorAll('.source-filter')];
const sources = [...document.querySelectorAll('#sourceGrid a')];

sourceFilters.forEach((button) => {
  button.addEventListener('click', () => {
    sourceFilters.forEach((item) => item.classList.toggle('active', item === button));
    sources.forEach((source) => {
      source.hidden = button.dataset.filter !== 'all' && source.dataset.category !== button.dataset.filter;
    });
  });
});

const quizQuestions = [
  {
    topic: 'MECHANICS',
    question: 'Why can a fluent LLM answer still be factually wrong?',
    options: ['Fluency and factual verification are different objectives', 'The tokenizer removes every factual word', 'Attention prevents access to earlier text'],
    correct: 0,
    explanation: 'Correct. Next-token prediction rewards plausible continuation. Truth requires evidence, retrieval, tools, or verification beyond fluent wording.'
  },
  {
    topic: 'SAMPLING',
    question: 'What does increasing temperature usually do?',
    options: ['Adds new knowledge to the model', 'Flattens token probabilities and increases variation', 'Expands the context window'],
    correct: 1,
    explanation: 'Temperature changes the probability distribution used for decoding. It does not retrain the model or extend its context.'
  },
  {
    topic: 'RAG',
    question: 'When is RAG a better first move than fine-tuning?',
    options: ['When answers need current, traceable private documents', 'When you want to change the model architecture', 'When no source material exists'],
    correct: 0,
    explanation: 'RAG is designed to bring external, updateable evidence into each request. Fine-tuning is usually better for recurring behavior or format.'
  },
  {
    topic: 'TRAINING',
    question: 'What is the main role of supervised instruction tuning?',
    options: ['Compress the model for phones', 'Teach a pretrained model useful response patterns', 'Search a vector database'],
    correct: 1,
    explanation: 'Supervised fine-tuning uses demonstrations to teach a base model how to respond to instructions in a useful format.'
  },
  {
    topic: 'SECURITY',
    question: 'Why is prompt injection primarily a system-design problem?',
    options: ['Long prompts always crash GPUs', 'Models cannot reliably distinguish trusted instructions from hostile text', 'Only open models can read prompts'],
    correct: 1,
    explanation: 'Applications must separate trust boundaries and restrict tool permissions because untrusted text can be interpreted as instructions.'
  },
  {
    topic: 'EVALUATION',
    question: 'What is the strongest evaluation set for an LLM feature?',
    options: ['A single public benchmark score', 'Realistic examples including failures and edge cases', 'Only examples the model already answers well'],
    correct: 1,
    explanation: 'Useful evaluation mirrors the real task and contains difficult, unsafe, and edge-case inputs—not only average-case examples.'
  }
];

let quizIndex = 0;
let quizScore = 0;
let quizAnswered = false;

function renderQuiz() {
  const question = quizQuestions[quizIndex];
  quizAnswered = false;
  document.getElementById('quizCurrent').textContent = String(quizIndex + 1).padStart(2, '0');
  document.getElementById('quizTopic').textContent = question.topic;
  document.getElementById('quizQuestion').textContent = question.question;
  document.getElementById('quizExplanation').textContent = '';
  document.getElementById('nextQuestion').hidden = true;
  const options = document.getElementById('quizOptions');
  options.replaceChildren();

  question.options.forEach((label, index) => {
    const button = document.createElement('button');
    button.type = 'button';
    button.className = 'quiz-option';
    button.textContent = label;
    button.addEventListener('click', () => answerQuiz(index));
    options.appendChild(button);
  });
}

function answerQuiz(selected) {
  if (quizAnswered) return;
  quizAnswered = true;
  const question = quizQuestions[quizIndex];
  const buttons = [...document.querySelectorAll('.quiz-option')];
  buttons.forEach((button, index) => {
    button.disabled = true;
    if (index === question.correct) button.classList.add('correct');
    if (index === selected && selected !== question.correct) button.classList.add('incorrect');
  });
  if (selected === question.correct) quizScore += 1;
  document.getElementById('quizExplanation').textContent = question.explanation;
  const next = document.getElementById('nextQuestion');
  next.hidden = false;
  next.textContent = quizIndex === quizQuestions.length - 1 ? 'See result →' : 'Next question →';
}

document.getElementById('nextQuestion').addEventListener('click', () => {
  if (quizIndex < quizQuestions.length - 1) {
    quizIndex += 1;
    renderQuiz();
    return;
  }

  const card = document.getElementById('quizCard');
  const percentage = Math.round((quizScore / quizQuestions.length) * 100);
  card.innerHTML = `
    <p class="quiz-topic">RESULT</p>
    <h3>${quizScore}/${quizQuestions.length} correct · ${percentage}%</h3>
    <p class="quiz-explanation">${percentage >= 80 ? 'Strong mental model. Keep testing it against real systems.' : 'Good start. Revisit the sections behind the missed questions, then try again.'}</p>
    <button id="restartQuiz" class="button button-primary" type="button">Restart quiz ↻</button>
  `;
  document.getElementById('restartQuiz').addEventListener('click', () => {
    window.location.reload();
  });
});
renderQuiz();

const progressSections = [...document.querySelectorAll('[data-progress]')];
const visitedSections = new Set();
const progressObserver = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) visitedSections.add(entry.target.id);
  });
  const percent = Math.round((visitedSections.size / progressSections.length) * 100);
  document.getElementById('progressLabel').textContent = `${percent}% explored`;
  document.getElementById('progressBar').style.width = `${percent}%`;
}, { threshold: 0.25 });

progressSections.forEach((section) => progressObserver.observe(section));
