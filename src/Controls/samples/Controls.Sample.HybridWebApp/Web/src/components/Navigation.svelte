<script lang="ts">
  import { availableFeatures } from '../catalog.ts'
  import type { Feature } from '../catalog.ts'
  import type { PlaygroundSession } from '../session.ts'

  let { session, compact }: { session: PlaygroundSession; compact: () => boolean } = $props()
  let search = $state('')
  let category = $state('')
  let navigation: HTMLElement
  const categories = [...new Set(availableFeatures.map(feature => feature.category))]
  let matches = $derived(availableFeatures.filter(feature =>
    (!category || feature.category === category) &&
    `${feature.title} ${feature.category} ${feature.description}`.toLowerCase().includes(search.toLowerCase())))

  function select(feature: Feature, button: HTMLButtonElement): void {
    session.select(feature)
    if (compact()) {
      navigation.ownerDocument.querySelector<HTMLElement>('#experiment')?.focus()
    } else {
      button.focus()
    }
  }
</script>

<aside class="catalog" aria-label="Feature catalog">
  <label for="search">Find a feature</label>
  <input id="search" type="search" placeholder="Search features…" autocomplete="off" bind:value={search} />
  <div class="filters">
    <div>
      <label for="category">Category</label>
      <select id="category" bind:value={category}>
        <option value="">All categories</option>
        {#each categories as name}
          <option value={name}>{name}</option>
        {/each}
      </select>
    </div>
  </div>
  <p id="catalog-count" class="secondary">
    {matches.length === availableFeatures.length
      ? `${availableFeatures.length} experiments`
      : `${matches.length} of ${availableFeatures.length} experiments`}
  </p>
  <nav id="feature-list" aria-label="Experiments" bind:this={navigation}>
    {#each matches as feature (feature.id)}
      <button
        type="button"
        data-feature={feature.id}
        aria-current={$session.selected.id === feature.id ? 'true' : 'false'}
        onclick={event => select(feature, event.currentTarget)}
      >
        <span>{feature.title}</span>
        <small>{feature.description.split(/\.\s/)[0]}</small>
      </button>
    {:else}
      <p>No matching features. Try a broader search or clear the filters.</p>
    {/each}
  </nav>
</aside>
